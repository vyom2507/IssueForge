# IssueForge

IssueForge is a role-based IT support ticket management platform built with ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, and ASP.NET Core Identity.

The application provides separate workflows for Users, Support Agents, and Administrators, allowing support requests to move through a structured ticket lifecycle from creation to resolution.

::Features::

  User
- Register and log in securely
- Create support tickets
- Set ticket priority and due date
- View personal tickets
- Edit ticket information
- Track ticket status
- Access a personal dashboard
- Users can only access their own tickets

  Support Agent
- Access a dedicated support dashboard
- View tickets assigned by administrators
- Review ticket details
- Update ticket status
- Manage assigned support requests
- Cannot access administrator-only pages

  Administrator
- Access an administrative dashboard
- View system-wide ticket statistics
- View all support tickets
- Manage users
- Assign application roles
- Promote users to Support Agent
- Assign tickets to support agents
- Monitor ticket progress

:Ticket Workflow:
 
Open
  ↓
Assigned
  ↓
In Progress
  ↓
Resolved
  ↓
Closed
