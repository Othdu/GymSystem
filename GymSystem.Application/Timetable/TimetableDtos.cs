using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.Application.Timetable;

public record TimetableItemResponse(
    int ClassSessionId,
    string Title,
    int InstructorId,
    DateTime StartTime, DateTime EndTime,
    int Capacity,
    int SpotsLeft);
