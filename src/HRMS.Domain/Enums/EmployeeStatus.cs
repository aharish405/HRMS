namespace HRMS.Domain.Enums;

public enum EmployeeStatus
{
    Draft = 0,        // Initial state - before offer acceptance
    Active = 1,       // Activated after offer acceptance
    OnNotice = 2,     // Employee has resigned
    Relieved = 3,     // Employee has left
    Terminated = 4    // Terminated by company
}
