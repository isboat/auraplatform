using Aura.Dashboard.Domain;using Aura.Dashboard.Repositories;using Aura.Dashboard.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Aura.Dashboard.Controllers;
[Authorize(Roles=Roles.Reviewer+","+Roles.Administrator)] public sealed class ReviewsController(IMediaRepository media,IModerationService moderation,IAssetStorage storage):ManagementController
{
 public async Task<IActionResult> Index(string? query,string? type,DateTime? from,DateTime? to,int page=1)=>View(await media.SearchAsync(query,type,ReviewStates.InReview,from,to,Math.Max(page,1),20));
 public async Task<IActionResult> Details(string id){var item=await media.FindAsync(id);if(item is null)return NotFound();ViewBag.PreviewUrl=storage.ReadUrl(item.ObjectKey);return View(item);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Decide(string id,bool approve,string? reason){try{await moderation.DecideAsync(id,approve,reason,Actor());return Success(approve?"Upload approved.":"Upload rejected.",nameof(Index));}catch(Exception e)when(e is DashboardRuleException or ConcurrencyException){return Failure(e,nameof(Index));}}
}
