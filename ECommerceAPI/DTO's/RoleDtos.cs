namespace ECommerceAPI.Models.DTOs
{
    public class CreateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class AssignRightsDto
    {
        public List<int> RightIds { get; set; } = new();
    }

    public class RoleRightDto
    {
        public int RightId { get; set; }
        public string RightName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}