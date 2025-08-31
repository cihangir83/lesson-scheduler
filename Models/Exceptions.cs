using System;

namespace LessonScheduler.Models
{
    public class ScheduleException : Exception
    {
        public ScheduleException(string message) : base(message) { }
        public ScheduleException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DataValidationException : ScheduleException
    {
        public DataValidationException(string message) : base(message) { }
        public DataValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SolverException : ScheduleException
    {
        public SolverException(string message) : base(message) { }
        public SolverException(string message, Exception innerException) : base(message, innerException) { }
    }
}