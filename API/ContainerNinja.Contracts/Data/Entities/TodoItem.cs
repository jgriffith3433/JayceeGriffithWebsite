using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Data.Entities
{
    public class TodoItem : AuditableEntity
    {
        public int ListId { get; set; }

        public string? Title { get; set; }

        public string? Note { get; set; }

        public PriorityLevel Priority { get; set; }

        public DateTime? Reminder { get; set; }

        public bool Done { get; set; }

        public virtual TodoList List { get; set; } = null!;
    }
}