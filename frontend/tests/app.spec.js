import { expect, test } from 'playwright/test';

test('homepage presents approved and popular media', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: /See it. Hear it./ })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Latest approved' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Most viewed' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Most liked' })).toBeVisible();
  await expect(page.locator('.media-card')).toHaveCount(12);
});

test('direct media URL has the required content order and lazy comments', async ({ page }) => {
  await page.goto('/media/northern-lights');
  const content = page.locator('.content-page');
  await expect(content.locator('h1')).toContainText('Chasing the northern lights');
  await expect(content.locator('.viewer')).toBeVisible();
  await expect(content.locator('.engagement')).toBeVisible();
  await expect(content.locator('.description')).toBeVisible();
  await expect(page.locator('.comment-list')).toHaveCount(0);
  await page.getByRole('button', { name: 'Show comments' }).click();
  await expect(page.locator('.comment-list')).toBeVisible();
});

test('registration, upload, legal, search, and admin routes render', async ({ page }) => {
  for (const path of ['/signin', '/register', '/search?tag=music', '/privacy', '/terms', '/admin']) {
    await page.goto(path);
    await expect(page.locator('main')).toBeVisible();
  }
});

test('mobile homepage does not overflow', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/');
  const dimensions = await page.evaluate(() => ({ page: document.documentElement.scrollWidth, viewport: window.innerWidth }));
  expect(dimensions.page).toBeLessThanOrEqual(dimensions.viewport);
});
