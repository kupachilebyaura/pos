using POS.API.Authorization;
using POS.Application.Constants;

[HttpPost]
[Permission(Permissions.UserCreate)]
public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request) { ... }

[HttpDelete("{id}")]
[Permission(Permissions.UserDelete)]
public async Task<IActionResult> Delete(int id) { ... }