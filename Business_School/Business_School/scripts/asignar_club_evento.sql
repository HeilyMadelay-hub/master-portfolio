-- ============================================
-- ASIGNAR CLUBS A EVENTOS
-- ============================================

-- 1. Ver los clubs del departamento de Marketing (DepartmentId = 2)
SELECT Id, Name, DepartmentId 
FROM Clubs 
WHERE DepartmentId = 2;

-- 2. Ver los eventos del departamento de Marketing
SELECT Id, Title, DepartmentId, StartDate 
FROM Events 
WHERE DepartmentId = 2
ORDER BY Id;

-- ============================================
-- 3. INSERTAR RELACIONES EN EventClubs
-- ============================================

-- Asumiendo que los clubs del departamento de Marketing son:
-- ClubId 3: Club de Marketing Digital
-- ClubId 4: Club de Ventas Estratégicas
-- (Ajusta los IDs según tu base de datos)

-- Asignar Club de Marketing Digital a varios eventos
INSERT INTO EventClubs (EventId, ClubId)
VALUES
(21, 3),  -- Evento Marketing 1
(22, 3),  -- Evento Marketing 2
(23, 3),  -- Evento Marketing 3
(26, 3);  -- Evento de Marketing

-- Asignar Club de Ventas Estratégicas a otros eventos
INSERT INTO EventClubs (EventId, ClubId)
VALUES
(24, 4),  -- Evento Marketing 4
(25, 4);  -- Evento Marketing 5

-- Asignar AMBOS clubs a un evento (evento conjunto)
INSERT INTO EventClubs (EventId, ClubId)
VALUES
(21, 4),  -- Evento Marketing 1 ahora tiene 2 clubs
(23, 4);  -- Evento Marketing 3 ahora tiene 2 clubs

-- ============================================
-- 4. VERIFICAR LAS ASIGNACIONES
-- ============================================

-- Ver eventos con sus clubs asignados
SELECT 
    e.Id AS EventId,
    e.Title,
    e.DepartmentId,
    c.Id AS ClubId,
    c.Name AS ClubName
FROM Events e
LEFT JOIN EventClubs ec ON e.Id = ec.EventId
LEFT JOIN Clubs c ON ec.ClubId = c.Id
WHERE e.DepartmentId = 2
ORDER BY e.Id, c.Name;

-- Ver cuántos clubs tiene cada evento
SELECT 
    e.Id AS EventId,
    e.Title,
    COUNT(ec.ClubId) AS TotalClubs
FROM Events e
LEFT JOIN EventClubs ec ON e.Id = ec.EventId
WHERE e.DepartmentId = 2
GROUP BY e.Id, e.Title
ORDER BY e.Id;

-- ============================================
-- 5. EJEMPLOS ADICIONALES
-- ============================================

-- Asignar un club específico a todos los eventos sin clubs
INSERT INTO EventClubs (EventId, ClubId)
SELECT e.Id, 3  -- ClubId 3
FROM Events e
LEFT JOIN EventClubs ec ON e.Id = ec.EventId
WHERE e.DepartmentId = 2 
  AND ec.EventId IS NULL;

-- Eliminar la asignación de un club a un evento específico
DELETE FROM EventClubs
WHERE EventId = 21 AND ClubId = 3;

-- Eliminar todos los clubs de un evento
DELETE FROM EventClubs
WHERE EventId = 22;

-- ============================================
-- 6. CONSULTA COMPLETA PARA VERIFICAR TODO
-- ============================================

SELECT 
    e.Id AS EventId,
    e.Title,
    e.StartDate,
    e.Capacity,
    d.Name AS DepartmentName,
    STRING_AGG(c.Name, ', ') AS Clubs,
    COUNT(DISTINCT ea.UserStudentId) AS RegisteredStudents
FROM Events e
INNER JOIN Departments d ON e.DepartmentId = d.Id
LEFT JOIN EventClubs ec ON e.Id = ec.EventId
LEFT JOIN Clubs c ON ec.ClubId = c.Id
LEFT JOIN EventAttendances ea ON e.Id = ea.EventId
WHERE e.DepartmentId = 2
GROUP BY e.Id, e.Title, e.StartDate, e.Capacity, d.Name
ORDER BY e.StartDate;

-- ============================================
-- SCRIPT COMPLETO: ASIGNAR CLUBS A EVENTOS
-- ============================================

-- Paso 1: Identificar IDs de clubs del departamento
DECLARE @ClubMarketing INT = (SELECT TOP 1 Id FROM Clubs WHERE DepartmentId = 2 ORDER BY Id);
DECLARE @ClubVentas INT = (SELECT TOP 1 Id FROM Clubs WHERE DepartmentId = 2 AND Id > @ClubMarketing ORDER BY Id);

-- Paso 2: Asignar clubs a eventos (distribución variada)
INSERT INTO EventClubs (EventId, ClubId)
SELECT EventId, ClubId FROM (VALUES
    -- Eventos con un solo club
    (21, @ClubMarketing),
    (22, @ClubVentas),
    (24, @ClubMarketing),
    (25, @ClubVentas),
    (26, @ClubMarketing),
    -- Evento 23 con ambos clubs (evento conjunto)
    (23, @ClubMarketing),
    (23, @ClubVentas)
) AS EventClubData(EventId, ClubId)
WHERE NOT EXISTS (
    SELECT 1 FROM EventClubs ec 
    WHERE ec.EventId = EventClubData.EventId 
      AND ec.ClubId = EventClubData.ClubId
);

-- Verificar resultado
SELECT 
    e.Id,
    e.Title,
    STRING_AGG(c.Name, ', ') WITHIN GROUP (ORDER BY c.Name) AS Clubs
FROM Events e
LEFT JOIN EventClubs ec ON e.Id = ec.EventId
LEFT JOIN Clubs c ON ec.ClubId = c.Id
WHERE e.DepartmentId = 2
GROUP BY e.Id, e.Title
ORDER BY e.Id;