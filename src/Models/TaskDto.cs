﻿namespace api_cleany_app.src.Models
{
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public List<string?> TaskImageUrl { get; set; }
        public string CreatedBy { get; set; }   
        public string TaskType { get; set; }
        public AreaDto Area { get; set; }
        public RoutineJson Routine { get; set; }
    }
}
