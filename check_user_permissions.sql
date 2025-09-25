-- Check if user "nilavanrajamani02@gmail.com" has CanModifyEventsData permission
-- This policy requires: Admin Role + Events_Modify claim with value "Modify"

DECLARE @UserEmail NVARCHAR(256) = 'nilavanrajamani02@gmail.com';

PRINT '=== USER PERMISSION CHECK FOR CanModifyEventsData ===';
PRINT 'Email: ' + @UserEmail;
PRINT '';

-- 1. Check if user exists
SELECT 
    'USER INFO' AS Section,
    Id as UserId,
    UserName,
    Email,
    EmailConfirmed,
    LockoutEnabled,
    AccessFailedCount
FROM AspNetUsers 
WHERE Email = @UserEmail;

-- 2. Check user roles
SELECT 
    'USER ROLES' AS Section,
    u.Email,
    r.Name as RoleName,
    r.Id as RoleId
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = @UserEmail;

-- 3. Check user claims (direct user claims)
SELECT 
    'USER CLAIMS' AS Section,
    u.Email,
    uc.ClaimType,
    uc.ClaimValue
FROM AspNetUsers u
INNER JOIN AspNetUserClaims uc ON u.Id = uc.UserId
WHERE u.Email = @UserEmail;

-- 4. Check role claims (claims inherited from roles)
SELECT 
    'ROLE CLAIMS' AS Section,
    u.Email,
    r.Name as RoleName,
    rc.ClaimType,
    rc.ClaimValue
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
INNER JOIN AspNetRoleClaims rc ON r.Id = rc.RoleId
WHERE u.Email = @UserEmail;

-- 5. Combined permission check for CanModifyEventsData
WITH UserPermissions AS (
    -- Get user info
    SELECT 
        u.Id as UserId,
        u.Email,
        -- Check if user has Admin role
        CASE WHEN EXISTS (
            SELECT 1 FROM AspNetUserRoles ur 
            INNER JOIN AspNetRoles r ON ur.RoleId = r.Id 
            WHERE ur.UserId = u.Id AND r.Name = 'Admin'
        ) THEN 1 ELSE 0 END AS HasAdminRole,
        
        -- Check if user has Events_Modify claim with value 'Modify'
        CASE WHEN EXISTS (
            SELECT 1 FROM AspNetUserClaims uc 
            WHERE uc.UserId = u.Id 
            AND uc.ClaimType = 'Events_Modify' 
            AND uc.ClaimValue = 'Modify'
        ) THEN 1 ELSE 0 END AS HasEventsModifyClaim,
        
        -- Check if user has Events_Modify claim through role
        CASE WHEN EXISTS (
            SELECT 1 FROM AspNetUserRoles ur 
            INNER JOIN AspNetRoles r ON ur.RoleId = r.Id 
            INNER JOIN AspNetRoleClaims rc ON r.Id = rc.RoleId
            WHERE ur.UserId = u.Id 
            AND rc.ClaimType = 'Events_Modify' 
            AND rc.ClaimValue = 'Modify'
        ) THEN 1 ELSE 0 END AS HasEventsModifyClaimThroughRole
    FROM AspNetUsers u
    WHERE u.Email = @UserEmail
)
SELECT 
    'PERMISSION SUMMARY' AS Section,
    Email,
    HasAdminRole,
    HasEventsModifyClaim,
    HasEventsModifyClaimThroughRole,
    CASE 
        WHEN HasAdminRole = 1 AND (HasEventsModifyClaim = 1 OR HasEventsModifyClaimThroughRole = 1) 
        THEN 'YES - Can access CanModifyEventsData policy'
        WHEN HasAdminRole = 0 
        THEN 'NO - Missing Admin role'
        WHEN HasEventsModifyClaim = 0 AND HasEventsModifyClaimThroughRole = 0
        THEN 'NO - Missing Events_Modify claim'
        ELSE 'NO - Unknown reason'
    END AS CanModifyEventsDataAccess
FROM UserPermissions;

-- 6. If user doesn't exist, show this message
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @UserEmail)
BEGIN
    PRINT 'USER NOT FOUND: ' + @UserEmail;
    PRINT 'Available users:';
    SELECT Email FROM AspNetUsers ORDER BY Email;
END

-- 7. Show what's needed if access is denied
PRINT '';
PRINT '=== REQUIREMENTS FOR CanModifyEventsData POLICY ===';
PRINT '1. User must have Admin role';
PRINT '2. User must have Events_Modify claim with value "Modify"';
PRINT '';
PRINT '=== TO GRANT ACCESS (if missing) ===';
PRINT '-- Add Admin role:';
PRINT 'INSERT INTO AspNetUserRoles (UserId, RoleId)';
PRINT 'SELECT u.Id, r.Id FROM AspNetUsers u, AspNetRoles r';
PRINT 'WHERE u.Email = ''' + @UserEmail + ''' AND r.Name = ''Admin'';';
PRINT '';
PRINT '-- Add Events_Modify claim:';
PRINT 'INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue)';
PRINT 'SELECT Id, ''Events_Modify'', ''Modify'' FROM AspNetUsers';
PRINT 'WHERE Email = ''' + @UserEmail + ''';';