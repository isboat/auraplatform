using Aura.Dashboard.Domain;using Aura.Dashboard.Repositories;using Aura.Dashboard.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Aura.Dashboard.Controllers;
[Authorize(Roles=Roles.Administrator)] public sealed class UsersController(IUserRepository users,IUserManagementService service):ManagementController
{public async Task<IActionResult> Index(string? query,string? role,bool? blocked,int page=1)=>View(await users.SearchAsync(query,role,blocked,Math.Max(page,1),20));public async Task<IActionResult> Details(string id){var u=await users.FindAsync(id);return u is null?NotFound():View(u);}
[HttpPost,ValidateAntiForgeryToken]public Task<IActionResult> Block(string id,bool blocked,string reason)=>Run(()=>service.SetBlockedAsync(id,blocked,reason,Actor()),blocked?"User blocked.":"User unblocked.");
[HttpPost,ValidateAntiForgeryToken]public Task<IActionResult> Reviewer(string id,bool enabled)=>Run(()=>service.SetReviewerAsync(id,enabled,Actor()),"Reviewer access updated.");
[HttpPost,ValidateAntiForgeryToken]public Task<IActionResult> Promote(string id)=>Run(()=>service.PromoteAdministratorAsync(id,Actor()),"Administrator access granted.");
[HttpPost,ValidateAntiForgeryToken]public Task<IActionResult> Reset(string id)=>Run(()=>service.RequestResetAsync(id,Actor()),"Password reset requested.");
[HttpPost,ValidateAntiForgeryToken]public Task<IActionResult> Delete(string id,string reason)=>Run(()=>service.DeleteAsync(id,reason,Actor()),"User deleted.");
private async Task<IActionResult> Run(Func<Task> action,string message){try{await action();return Success(message,nameof(Index));}catch(DashboardRuleException e){return Failure(e,nameof(Index));}}}
