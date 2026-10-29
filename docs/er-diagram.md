# FMMS — Database ER Diagram

```mermaid
erDiagram
    MACHINE    ||--o{ WORKORDER : has
    TECHNICIAN |o--o{ WORKORDER : has
    MACHINE ||--o{ SENSORREADING : records
    MACHINE ||--o{ ALERT : triggers
    MACHINE ||--o{ MAINTENANCESCHEDULE : has
    ALERT |o--o{ WORKORDER : creates  

    MACHINE {
        int Id PK
        string Code
        string Name
        string Model
        string Location
        DateTime InstallDate
        MachineStatus Status
    }
    TECHNICIAN {
        int Id PK
        string Name
        string Specialization
    }
    WORKORDER {
        int Id PK
        int MachineId FK
        int TechnicianId FK
        int AlertId FK
        string Description
        UrgencyStatus Urgency
        WorkOrderType Type
        DateTime CreatedDate
        DateTime ClosedDate
    }
    SENSORREADING {
        int Id PK
        int MachineId FK
        SensorChannel Channel
        double Value
        DateTime DateTime
    }
    ALERT {
        int Id PK
        int MachineId FK
        SensorChannel Channel
        double Value
        double Threshold
        AlertSeverity Severity
        DateTime DetectedAt
    }
    MAINTENANCESCHEDULE {
        int Id PK
        int MachineId FK
        int IntervalDays
        DateTime NextDueDate
        DateTime LastCompletedDate
        bool IsActive
    }
    
```