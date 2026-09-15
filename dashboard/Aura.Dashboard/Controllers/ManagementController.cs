using System.Security.Claims;
using Aura.Dashboard.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Dashboard.Controllers;
public abstract class ManagementController:Controller
{
    protected StaffActor Actor()=>new(User.FindFirstValue(ClaimTypes.NameIdentifier)!,User.Identity!.Name!,User.FindFirstValue(ClaimTypes.Email)!,User.IsInRole(Roles.Administrator)?Roles.Administrator:Roles.Reviewer,HttpContext.TraceIdentifier);
    protected IActionResult Success(string message,string action){TempData["Success"]=message;return RedirectToAction(action);}
    protected IActionResult Failure(Exception ex,string action){TempData["Error"]=ex.Message;return RedirectToAction(action);}
}
