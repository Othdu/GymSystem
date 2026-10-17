using System;
using System.Collections.Generic;
using System.Text;
using GymSystem.Domain.Enums;
namespace GymSystem.Application.Instructors;

public record AttendeeResponce(int MemberId, BookingStatus Status);

public record InstructorClassResponse(int ClassSessionId, string Title, DateTime StartTime, DateTime EndTime, int Capacity, List<AttendeeResponce> Attendee);
public record MarkAttendanceRequest(int ClassSessionId, int MemberId, BookingStatus Outcome);
public record MarkAttendanceResponse(int ClassSessionId, int MemberId, BookingStatus Status);