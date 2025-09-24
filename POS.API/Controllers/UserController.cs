using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Application.DTOs;
// using POS.API.Authorization;
// using POS.Application.Constants;

namespace POS.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		[HttpPost]
		//[Permission(Permissions.UserCreate)]
		public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
		{
			// ... lógica de creación ...
			return Ok();
		}

		[HttpDelete("{id}")]
		//[Permission(Permissions.UserDelete)]
		public async Task<IActionResult> Delete(int id)
		{
			// ... lógica de borrado ...
			return Ok();
		}
	}
}