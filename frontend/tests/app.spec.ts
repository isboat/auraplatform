import { expect, test, type Page } from 'playwright/test';

const video = { id: 'northern-lights', title: 'Chasing the northern lights', description: 'A quiet journey beneath the dancing skies.', tags: ['travel', 'nature'], mediaType: 'video', reviewStatus: 'Approved', createdAt: '2026-09-13T00:00:00Z', ownerName: 'Maya Chen', views: 128400, likes: 8200, dislikes: 91, comments: 2, url: 'https://media.invalid/video.mp4' };
const image = { ...video, id: 'ceramic-light', title: 'Light and quiet spaces', mediaType: 'image', url: 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==' };
const audio = { ...video, id: 'after-rain', title: 'After the rain', mediaType: 'audio', url: 'https://media.invalid/audio.mp3' };
const items = [video, image, audio, { ...video, id: 'city-motion', title: 'A city in motion' }];

async function mockApi(page: Page) {
  await page.route('http://localhost:5080/api/**', async route => {
    const url = new URL(route.request().url());
    if (url.pathname === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    if (url.pathname === '/api/media/home') return route.fulfill({ json: { latest: items, mostViewed: items, mostLiked: items } });
    if (url.pathname === '/api/media/search') return route.fulfill({ json: items });
    if (url.pathname.endsWith('/comments')) return route.fulfill({ json: [{ id: 'comment', mediaId: video.id, userId: 'user', userName: 'Jamie', body: 'Beautiful work.', createdAt: '2026-09-14T09:42:00Z' }] });
    if (url.pathname === '/api/media/missing') return route.fulfill({ status: 404, json: { detail: 'Media was not found.' } });
    const id = url.pathname.split('/').at(-1);
    return route.fulfill({ json: id === audio.id ? audio : id === 'in-review' ? { ...video, id, reviewStatus: 'InReview', url: null } : video });
  });
}

test.beforeEach(async ({ page }) => mockApi(page));

test('registration offers an optional phone number and sends it to the API', async ({ page }) => {
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    expect(route.request().postDataJSON()).toEqual({ name: 'Jamie', email: 'jamie@example.com', password: 'password', phone: '+1 202 555 0147' });
    return route.fulfill({ status: 202, json: { message: 'Check your email to complete account setup.' } });
  });

  await page.goto('/register', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('Name').fill('Jamie');
  await page.getByLabel('Email address').fill('jamie@example.com');
  await page.getByLabel('Phone number (optional)').fill('+1 202 555 0147');
  await page.getByLabel('Password').fill('password');
  await page.getByRole('button', { name: 'Create account' }).click();

  await expect(page.getByRole('heading', { name: 'Check your inbox' })).toBeVisible();
});

test('homepage loads API media and remains responsive', async ({ page }) => {
  await page.goto('/', { waitUntil: 'domcontentloaded' });
  await expect(page.getByRole('heading', { name: 'Latest approved' })).toBeVisible();
  await expect(page.locator('.media-card')).toHaveCount(12);
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
});

test('video player and lazy backend comments render', async ({ page }) => {
  await page.goto('/media/northern-lights', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('video[controls]')).toHaveCount(1);
  await expect(page.locator('.comment-list')).toHaveCount(0);
  await page.getByRole('button', { name: 'Show comments' }).click();
  await expect(page.locator('.comment-list')).toContainText('Beautiful work.');
});

test('share button opens the native share dialog with media details', async ({ page }) => {
  await page.addInitScript(() => {
    Object.defineProperty(navigator, 'share', {
      configurable: true,
      value: async (data: ShareData) => { (window as Window & { sharedMedia?: ShareData }).sharedMedia = data; }
    });
  });
  await page.goto('/media/northern-lights', { waitUntil: 'domcontentloaded' });

  await page.getByRole('button', { name: 'Share media' }).click();

  await expect(page.getByRole('button', { name: 'Shared' })).toBeVisible();
  const shared = await page.evaluate(() => (window as Window & { sharedMedia?: ShareData }).sharedMedia);
  expect(shared).toEqual({
    title: video.title,
    text: video.description,
    url: 'http://127.0.0.1:4173/media/northern-lights'
  });
});

test('share button copies the content link when native sharing is unavailable', async ({ page }) => {
  await page.addInitScript(() => {
    Object.defineProperty(navigator, 'share', { configurable: true, value: undefined });
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: { writeText: async (value: string) => { (window as Window & { copiedLink?: string }).copiedLink = value; } }
    });
  });
  await page.goto('/media/northern-lights', { waitUntil: 'domcontentloaded' });

  await page.getByRole('button', { name: 'Share media' }).click();

  await expect(page.getByRole('button', { name: 'Link copied' })).toBeVisible();
  expect(await page.evaluate(() => (window as Window & { copiedLink?: string }).copiedLink))
    .toBe('http://127.0.0.1:4173/media/northern-lights');
});

test('audio player and unavailable media states render', async ({ page }) => {
  await page.goto('/media/after-rain', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('audio[controls]')).toHaveCount(1);
  await page.goto('/media/missing', { waitUntil: 'domcontentloaded' });
  await expect(page.getByRole('heading', { name: 'Media not found' })).toBeVisible();
  await page.goto('/media/in-review', { waitUntil: 'domcontentloaded' });
  await expect(page.getByRole('heading', { name: 'Media unavailable' })).toBeVisible();
});

test('backend failures display a dismissible UI message', async ({ page }) => {
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', route => route.fulfill({ status: 503, json: { detail: 'The media service is temporarily unavailable.' } }));
  await page.goto('/', { waitUntil: 'domcontentloaded' });
  await expect(page.getByRole('alert')).toContainText('The media service is temporarily unavailable.');
  await page.getByRole('button', { name: 'Dismiss error' }).click();
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('upload page immediately shows progress while an upload starts', async ({ page }) => {
  await page.addInitScript(() => {
    localStorage.setItem('aura-user', JSON.stringify({ id: 'user', name: 'Jamie', email: 'jamie@example.com', isAdministrator: false }));
    localStorage.setItem('aura-token', 'test-token');
  });
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    if (path === '/api/media/uploads') {
      await new Promise(resolve => setTimeout(resolve, 1_000));
      return route.fulfill({ json: { mediaId: 'media', uploadId: 'upload', partUrls: [] } });
    }
    return route.fulfill({ json: { message: 'Upload complete.' } });
  });

  await page.goto('/upload', { waitUntil: 'domcontentloaded' });
  await page.locator('input[type="file"]').setInputFiles('tests/fixtures/sample.mp4');
  await page.getByRole('button', { name: 'Upload for review' }).click();

  const uploadButton = page.getByRole('button', { name: 'Preparing upload…' });
  await expect(uploadButton).toBeDisabled();
  await expect(uploadButton.locator('.upload-spinner')).toBeVisible();
  await expect(page.getByRole('progressbar', { name: 'Media upload progress' })).toHaveAttribute('aria-valuenow', '0');
  await expect(page.getByText('Please keep this page open while your media is being uploaded.')).toBeVisible();
});

test('password reset request uses a privacy-preserving confirmation', async ({ page }) => {
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    return route.fulfill({ status: 202, json: { message: 'If an account exists for that verified email address, a password reset link has been sent.' } });
  });

  await page.goto('/signin', { waitUntil: 'domcontentloaded' });
  await page.getByRole('link', { name: 'Forgot your password?' }).click();
  await page.getByLabel('Email address').fill('person@example.com');
  await page.getByRole('button', { name: 'Send reset link' }).click();

  await expect(page.getByRole('heading', { name: 'Check your inbox' })).toBeVisible();
  await expect(page.getByText('If an account exists for that verified email address')).toBeVisible();
});

test('password reset link lets a user choose and confirm a new password', async ({ page }) => {
  await page.addInitScript(() => {
    localStorage.setItem('aura-user', JSON.stringify({ id: 'user', name: 'Jamie', email: 'jamie@example.com', isAdministrator: false }));
    localStorage.setItem('aura-token', 'old-session-token');
  });
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    expect(route.request().postDataJSON()).toEqual({ token: 'one-time-token', password: 'new-password', confirmPassword: 'new-password' });
    return route.fulfill({ json: { message: 'Your password has been reset. Sign in with your new password.' } });
  });

  await page.goto('/reset-password?token=one-time-token', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('New password', { exact: true }).fill('new-password');
  await page.getByLabel('Confirm new password').fill('new-password');
  await page.getByRole('button', { name: 'Reset password' }).click();

  await expect(page.getByRole('heading', { name: 'Password reset' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Continue to sign in' })).toBeVisible();
  expect(await page.evaluate(() => localStorage.getItem('aura-token'))).toBeNull();
});

test('password reset catches mismatched confirmation before calling the API', async ({ page }) => {
  let resetCalls = 0;
  await page.unroute('http://localhost:5080/api/**');
  await page.route('http://localhost:5080/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/configuration') return route.fulfill({ json: { id: 'platform', registrationEnabled: true, uploadsEnabled: true } });
    resetCalls += 1;
    return route.fulfill({ json: { message: 'Unexpected request.' } });
  });

  await page.goto('/reset-password?token=one-time-token', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('New password', { exact: true }).fill('new-password');
  await page.getByLabel('Confirm new password').fill('different-password');
  await page.getByRole('button', { name: 'Reset password' }).click();

  await expect(page.getByRole('alert')).toHaveText('Passwords do not match.');
  expect(resetCalls).toBe(0);
});
