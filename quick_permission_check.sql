-- Quick check for CanModifyEventsData permission
-- User: nilavanrajamani02@gmail.com

-- 1. User exists?
SELECT 'User exists: ' + CASE WHEN COUNT(*) > 0 THEN 'YES' ELSE 'NO' END as Result
FROM AspNetUsers WHERE Email = 'nilavanrajamani02@gmail.com';

-- 2. Has Admin role?
SELECT 'Has Admin role: ' + CASE WHEN COUNT(*) > 0 THEN 'YES' ELSE 'NO' END as Result
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'nilavanrajamani02@gmail.com' AND r.Name = 'Admin';

-- 3. Has Events_Modify claim?
SELECT 'Has Events_Modify claim: ' + CASE WHEN COUNT(*) > 0 THEN 'YES' ELSE 'NO' END as Result
FROM AspNetUsers u
INNER JOIN AspNetUserClaims uc ON u.Id = uc.UserId
WHERE u.Email = 'nilavanrajamani02@gmail.com' 
AND uc.ClaimType = 'Events_Modify' 
AND uc.ClaimValue = 'Modify';

-- 4. Final result
SELECT CASE 
    WHEN EXISTS (
        SELECT 1 FROM AspNetUsers u
        INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
        WHERE u.Email = 'nilavanrajamani02@gmail.com' AND r.Name = 'Admin'
    ) AND EXISTS (
        SELECT 1 FROM AspNetUsers u
        INNER JOIN AspNetUserClaims uc ON u.Id = uc.UserId
        WHERE u.Email = 'nilavanrajamani02@gmail.com' 
        AND uc.ClaimType = 'Events_Modify' 
        AND uc.ClaimValue = 'Modify'
    )
    THEN '✅ YES - Has CanModifyEventsData permission'
    ELSE '❌ NO - Missing required permissions'
END as 'CanModifyEventsData Permission';

-- Show what user actually has
SELECT 
    u.Email,
    STRING_AGG(r.Name, ', ') as Roles,
    (SELECT STRING_AGG(uc.ClaimType + '=' + uc.ClaimValue, ', ') 
     FROM AspNetUserClaims uc WHERE uc.UserId = u.Id) as Claims
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'nilavanrajamani02@gmail.com'
GROUP BY u.Id, u.Email;