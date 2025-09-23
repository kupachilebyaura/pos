services.AddAuthorization(options =>
{
    options.AddPolicy("Permission:user:create", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permissions.UserCreate)));
    // Agrega políticas para cada permiso necesario
});
services.AddSingleton<IAuthorizationHandler, PermissionHandler>();