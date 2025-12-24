SELECT Id AS DepartmentId, Name
FROM Departments
WHERE ManagerUserId = 2;

SELECT * FROM Events WHERE DepartmentId = 2;

INSERT INTO Events (Title, Description, StartDate, EndDate, DepartmentId, Capacity, DefaultPointsReward)
VALUES ('Evento de Marketing', 'Evento de prueba', GETDATE() + 1, GETDATE() + 2, 2, 50, 10);

UPDATE Events
SET DepartmentId = 2
WHERE DepartmentId IS NULL;


INSERT INTO Events (Title, Description, StartDate, EndDate, DepartmentId, Capacity, DefaultPointsReward)
VALUES 
('Evento Marketing 1', 'Evento de prueba 1', GETDATE() + 1, GETDATE() + 1, 2, 50, 10),
('Evento Marketing 2', 'Evento de prueba 2', GETDATE() + 2, GETDATE() + 2, 2, 40, 10),
('Evento Marketing 3', 'Evento de prueba 3', GETDATE() + 3, GETDATE() + 3, 2, 30, 10),
('Evento Marketing 4', 'Evento de prueba 4', GETDATE() + 4, GETDATE() + 4, 2, 60, 10),
('Evento Marketing 5', 'Evento de prueba 5', GETDATE() + 5, GETDATE() + 5, 2, 50, 10);


select * from AspNetUsers

-- Inscribir estudiantes a los eventos de Marketing con puntos y asistencia
INSERT INTO EventAttendances (EventId, UserStudentId, RegisteredAt, PointsAwarded, HasAttended)
VALUES
-- Evento 1
(1, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- María Pérez
(1, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- Jorge López
(1, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- Elena Díaz

-- Evento 2
(2, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0), -- Ricardo Torres
(2, 11, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0), -- Claudia Ramos

-- Evento 3
(3, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(3, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(3, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),

-- Evento 4
(4, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(4, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(4, 11, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),

-- Evento 5
(5, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(5, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(5, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0);


SELECT Id, Title, DepartmentId FROM Events;

-- Inscribir estudiantes a los eventos de Marketing (DepartmentId = 2)
INSERT INTO EventAttendances (EventId, UserStudentId, RegisteredAt, PointsAwarded, HasAttended)
VALUES
-- Evento 21
(21, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- María Pérez
(21, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- Jorge López
(21, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),  -- Elena Díaz

-- Evento 22
(22, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0), -- Ricardo Torres
(22, 11, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0), -- Claudia Ramos

-- Evento 23
(23, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(23, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(23, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),

-- Evento 24
(24, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(24, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(24, 11, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),

-- Evento 25
(25, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(25, 8, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(25, 10, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),

-- Evento 26
(26, 7, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(26, 9, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0),
(26, 11, GETDATE(), FLOOR(RAND(CHECKSUM(NEWID())) * 16) + 5, 0);

