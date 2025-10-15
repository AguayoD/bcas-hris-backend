
namespace Models.Enums
{
    public enum StoredProcedures
    {
        //---Activity Logs---//
        tblActivityLogs_GetAll,
        tblActivityLogs_GetById,
        tblActivityLogs_Insert,
        tblActivityLogs_Update,
        tblActivityLogs_Delete,

        //---Backup Records---//
        tblBackupRecords_GetAll,
        tblBackupRecords_GetById,
        tblBackupRecords_Insert,
        tblBackupRecords_Update,
        tblBackupRecords_Delete,

        //---Contracts---//
        tblContracts_GetAll,
        tblContracts_GetById,
        tblContracts_Insert,
        tblContracts_Update,
        tblContracts_Delete,

        //---Department---//
        tblDepartment_GetAll,
        tblDepartment_GetById,
        tblDepartment_Insert,
        tblDepartment_Update,
        tblDepartment_Delete,

        //---Employee---//
        tblEmployees_GetAll,
        tblEmployees_GetById,
        tblEmployees_Insert,
        tblEmployees_Update,
        tblEmployees_Delete,

        //---Notifications---//
        tblNotifications_GetAll,
        tblNotifications_GetById,
        tblNotifications_Insert,
        tblNotifications_Update,
        tblNotifications_Delete,

        //---Positions---//
        tblPositions_GetAll,
        tblPositions_GetById,
        tblPositions_Insert,
        tblPositions_Update,
        tblPositions_Delete,

        //--tblUser--//
        tblUsers_GetAll,
        tblUsers_GetById,
        tblUsers_Insert,
        tblUsers_GetByUsername,
        tblUsers_Update,
        tblUsers_Delete,

        //--tblRole--//
        tblRoles_GetByUserId,

        //forget
        tblUsers_GetByEmail,           // ✅ New
        tblUsers_SaveResetToken,       // ✅ New
        tblUsers_GetByResetToken,      // ✅ New
        tblUsers_ClearResetToken,      // ✅ New
        tblUsers_UpdatePassword,       // ✅ New

        //---Educational Attainment---//
        tblEducationalAttainment_GetAll,
        tblEducationalAttainment_GetById,
        tblEducationalAttainment_Insert,
        tblEducationalAttainment_Update,
        tblEducationalAttainment_Delete,

        //---Employment Status---//
        tblEmploymentStatus_GetAll,
        tblEmploymentStatus_GetById,
        tblEmploymentStatus_Insert,
        tblEmploymentStatus_Update,
        tblEmploymentStatus_Delete,

        // Role stored procedures
        tblRoles_GetAll,
        tblRoles_GetById,
        tblRoles_Insert,
        tblRoles_Update,
        tblRoles_DeleteById,
    }
}
