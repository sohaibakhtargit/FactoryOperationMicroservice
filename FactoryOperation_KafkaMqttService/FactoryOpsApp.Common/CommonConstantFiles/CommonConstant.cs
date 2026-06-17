namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.CommonConstantFiles
{
    public class CommonConstant
    {
        public static class StatusCode
        {
            public const string Success = "200";
            public const string BadRequest = "400";
            public const string Forbidden = "403";
            public const string NotFound = "404";
            public const string Error = "500";
            public const string MultiStatus = "207";
        }
        //SuperAdmin.....
        public static class AnnouncementStatusMessage
        {
            public const string AnnouncementAdded = "Announcement created successfully and emails sent (if applicable)";
            public const string AnnouncementFetched = "Announcement Data fetched successfully.";
            public const string AnnouncementUpdated = "Announcement updated successfully and emails sent (if applicable).";
            public const string AnnouncementDeleted = "Announcement deleted successfully.";
            public const string NoRecordsFound = "Announcement not found..";
        }
        public static class AuditStatusMessage
        {
            public const string AuditFetched = "Successfully retrieved all audit records";
        }
        public static class GlobalDropdownStatusMessage
        {
            public const string GlobalDropdownFetched = "Successfully retrieved all dropdowns";
            public const string ModuleDropdownFetched = "Successfully retrieved dropdowns for module:";
            public const string ModuleSubModuleDropdown = "Successfully retrieved dropdowns for:";
        }
        public static class IntegrationSettingsStatusMessage
        {
            public const string NoRecordsFound = "Integration setting not found";
            public const string NoCategoryRecordsFound = "No integration settings found in category";
            public const string DataFetched = "Fetched integration setting successfully";
            public const string CategoryDataFetched = "successfully Fetched integration settings for category";
            public const string IntegrationAdded = "Integration setting created successfully.";
            public const string IntegrationUpdated = "Integration setting updated successfully.";
            public const string IntegrationDeleted = "Integration setting deleted successfully.";
        }
        public static class SuperAdminDashboardStatusMessage
        {
            public const string DataFetched = "User count fetched successfully.";
        }
        public static class TenantMigrationStatusMessage
        {
            public const string MigrationAppilied = "All migrations applied successfully";
        }
        public static class SuperAdminSupportFeedbackStatusMessage
        {
            public const string FetchFailed = "An error occurred while fetching support feedbacks.";
            public const string DataFetched = "Support feedbacks retrieved successfully";
        }
        public static class SuperAdminSupportTicketStatusMessage
        {
            public const string DataFetched = "Support tickets retrieved successfully";
            public const string NoRecordsFound = "Support ticket not found.";
            public const string DataUpdated = "Support ticket updated successfully.";
            public const string UpdateFailed = "An error occurred while updating the support ticket.";
        }
        public static class TenantIsolationStatusMessage
        {
            public const string AddAudit = "Audit compliance metrics created successfully.";
            public const string AuditUpdated = "Audit compliance metrics updated successfully.";
            public const string AuditFetched = "Audit compliance metrics retrieved successfully";
            public const string NoRecordsFound = "No Data Found";
            public const string InternalServerError = "Internal Server Error";
        }
        public static class BackupStatusMessage
        {
            public const string AddBackup = "Backup created successfully";
            public const string BackupRestored = "Backup restored successfully";
            public const string AddBackupFailed = "Backup failed";
            public const string BackupFetched = "Successfully get All backups Records";
            public const string BackupRestoredFailed = "Backup Restore failed";
            public const string BackupFetchedFailed = "Backup fetch failed:";
            public const string NoRecordsFound = "Backup job not found";
            public const string BackupDeleted = "Backup job deleted successfully";
            public const string BackupDeleteFailed = "Failed to delete backup job";
        }
        public static class SuperAdminTeamStatusMessage
        {
            public const string DataFetched = "Teams retrieved successfully.";
            public const string FetchFailed = "Failed to retrieve teams.";

        }
        public static class CompanyBrandingInfoStatusMessage
        {
            public const string BadRequest = "Company name already exists.";
            public const string AddCompanyBranding = "Company branding created successfully.";
            public const string CompanyBrandingUpdated = "Company branding updated successfully.";
            public const string UpdatedFailed = "Error updating company branding";
            public const string AddCompanyBrandingFailed = "Error creating company branding";
            public const string FetchFailed = "Company branding not found.";
            public const string DataFetched = "Company branding data retrieved successfully.";
            public const string Error = "Error fetching company brandings";

        }
        public static class TenantStatusMessage
        {
            public const string BadRequest = "Tenant Already Exists.";
            public const string DeletedBadRequest = "Tenant already Deleted";
            public const string DBError = "Tenant added, but failed to create tenant database.";
            public const string EmailError = "User created but email failed: due to some Server issue";
            public const string TenantAdded = "Successfully added tenant and created tenant DB.";
            public const string TenantUpdated = "Successfully Updated Tenant.";
            public const string TenantDeleted = "Tenant Deleted Successfully";
            public const string TenantModulesUpdated = "Tenant modules updated successfully.";
            public const string DataFetched = "Successfully retrieved all tenants";
            public const string AllModuleDataFetched = "All Module Successfully retrieved";
            public const string PermissionsError = "Error fetching permissions";
            public const string FetchFailed = "Record Not Found.";
            public const string TenantFailed = "Tenant not found.";
            public const string TenantNameExists = "Tenant name already exists.";
            public const string AdminEmailExists = "Admin email already exists.";
            public const string StatusUpdated = "Status Updated Successfully";
            public const string PermissionBadRequest = "Tenant already force logged out";
            public const string LogOutRequest = "Forcelogout Successfully";
            public const string TenantAndAdminLoginsActionSuccess = "Tenant and its admin logins {0} successfully";
            public const string ToggleSuspensionFailed = "Failed to toggle suspension: {0}";

        }
        public static class GlobalUserStatusMessage
        {
            public const string Success = "Operation completed successfully.";
            public const string DataFetched = "Successfully retrieved data.";
            public const string UsersFetched = "Successfully retrieved all users.";
            public const string SuspendUsersFetched = "Successfully retrieved all suspended users Details.";
            public const string UserDetailsFetched = "Successfully retrieved user details.";
            public const string SuperAdminDetailsFetched = "Successfully retrieved super admin details.";
            public const string TenantDetailsFetched = "Successfully retrieved tenant details.";
            public const string ProfileUpdated = "Profile updated successfully.";
            public const string ForceLogoutSuccess = "Force logout successfully.";
            public const string UserSuspended = "User suspended successfully.";
            public const string UserUnsuspended = "User unsuspended successfully.";
            public const string UserNotFound = "User not found.";
            public const string TenantNotFound = "Tenant not found.";
            public const string TenantAdminNotFound = "Tenant admin user not found.";
            public const string TenantUserNotFound = "Tenant user not found.";
            public const string FactoryUserInfoNotFound = "Factory user information not found.";
            public const string EmailExistsInTenant = "Email already exists in this tenant.";
            public const string EmailExistsGlobally = "Email already exists globally.";
            public const string AlreadyForceLoggedOut = "User already force logged out.";
            public const string DBError = "Database operation failed.";
            public const string ProfileUpdateFailed = "Error updating profile.";
            public const string ToggleSuspendFailed = "Failed to toggle user suspension.";
            public const string ForceLogoutFailed = "Error occurred while force logging out the user.";
            public const string DataFetchFailed = "Failed to retrieve data.";
            public const string AuditForceLogout = "Force-Logout applied to user.";
            public const string AuditUserProfileUpdated = "Profile for user '{0}' updated with {1}.";
            public const string AuditSuperAdminProfileUpdated = "Profile for super admin '{0}' updated with {1}.";
            public const string AuditTenantProfileUpdated = "Profile for tenant '{0}' updated with {1}.";
            public const string AuditUserSuspension = "User {0}ed: {1}"; // Suspend/Unsuspend dynamically
            public const string UnknownError = "An unexpected error occurred.";
        }

        //Tenant Admin Repository Status Messages

        public static class ChallengeStatusMessage
        {
            public const string ChallengeAdded = "Challenge added successfully.";
            public const string ChallengeBoardFetched = "Fetched challenge board successfully.";
            public const string ChallengeAddFailed = "Failed to add challenge.";
            public const string ChallengeBoardFetchFailed = "Failed to fetch challenge board.";
        }
        public static class PointAssignmentStatusMessage
        {
            public const string PointAssignmentAdded = "Point assignment created successfully.";
            public const string PointAssignmentAddFailed = "Failed to create point assignment.";
            public const string PointAssignmentUpdated = "Point assignment updated successfully.";
            public const string PointAssignmentUpdateFailed = "Failed to update point assignment.";
            public const string PointAssignmentDeleted = "Point assignment deleted successfully.";
            public const string PointAssignmentDeleteFailed = "Failed to delete point assignment.";
            public const string PointAssignmentsFetched = "Point assignments retrieved successfully.";
            public const string PointAssignmentsFetchFailed = "Failed to retrieve point assignments.";
            public const string PointAssignmentNotFound = "Point assignment not found.";
        }
        public static class TrainingModuleStatusMessage
        {
            public const string TrainingModuleAdded = "Training module created successfully.";
            public const string TrainingModuleAddFailed = "Failed to create training module.";
            public const string TrainingModuleAlreadyExists = "Training module already exists.";
            public const string TrainingModuleUpdated = "Training module updated successfully.";
            public const string TrainingModuleUpdateFailed = "Failed to update training module.";
            public const string TrainingModuleNotFound = "Training module not found.";
            public const string TrainingModuleDeleted = "Training module deleted successfully.";
            public const string TrainingModuleDeleteFailed = "Failed to delete training module.";
            public const string TrainingModulesFetched = "Training modules retrieved successfully.";
            public const string TrainingModulesFetchFailed = "Failed to retrieve training modules.";
        }
        public static class UserBadgeStatusMessage
        {
            public const string UserBadgeAdded = "User badge added successfully.";
            public const string UserBadgeAddFailed = "Failed to add user badge.";
            public const string UserAlreadyHasBadge = "User already has this badge.";
            public const string BadgeAwarded = "Badge awarded successfully.";
            public const string BadgeAwardFailed = "Failed to award badge.";
            public const string UserBadgeUpdated = "User badge progress updated successfully.";
            public const string UserBadgeUpdateFailed = "Failed to update user badge progress.";
            public const string UserBadgeNotFound = "User badge not found.";
            public const string UserBadgeDeleted = "User badge deleted successfully.";
            public const string UserBadgeDeleteFailed = "Failed to delete user badge.";
            public const string UserBadgesFetched = "User badges retrieved successfully.";
            public const string UserBadgesFetchFailed = "Failed to retrieve user badges.";
            public const string UserBadgeFetched = "User badge retrieved successfully.";
            public const string UserBadgeFetchFailed = "Failed to retrieve user badge.";
        }
        public static class TenantAdminDashboardStatusMessage
        {
            public const string UsersFetched = "User count fetched successfully.";
            public const string ActiveUsersFetched = "Active Users Count (Current Month) fetched successfully.";
            public const string SuspendedUsersFetched = "Suspended Users Count fetched successfully.";
            public const string ActiveTicketsFetched = "Active Support Ticket Count fetched successfully.";
            public const string NewTicketsFetched = "New Ticket Count fetched successfully.";
            public const string TicketsInProgressFetched = "Tickets In Progress Count fetched successfully.";
            public const string TicketsResolvedFetched = "Tickets Resolved Today Count fetched successfully.";
            public const string CriticalPriorityFetched = "Critical Priority Ticket Count fetched successfully.";
            public const string TeamsFetched = "Total Team Count fetched successfully.";
            public const string AdminCountFetched = "User Role Admin Count fetched successfully.";
            public const string SupervisorCountFetched = "User Role Supervisor Count fetched successfully.";
            public const string TechnicianCountFetched = "User Role Technician Count fetched successfully.";
            public const string OperatorCountFetched = "User Role Operator Count fetched successfully.";
        }

        public static class FactoryGroupStatusMessage
        {
            public const string GroupCreated = "Group created successfully.";
            public const string GroupCreateFailed = "Failed to create group.";
            public const string GroupUpdated = "Group updated successfully.";
            public const string GroupUpdateFailed = "Failed to update group.";
            public const string GroupNotFound = "Group not found.";
            public const string GroupDeleted = "Group deleted successfully.";
            public const string GroupDeleteFailed = "Failed to delete group.";
            public const string GroupsFetched = "Groups retrieved successfully.";
            public const string GroupFetched = "Group retrieved successfully.";
            public const string GroupsFetchFailed = "Failed to retrieve groups.";
            public const string GroupFetchFailed = "Failed to retrieve group.";
        }
        public static class FactoryLocationStatusMessage
        {
            public const string LocationAdded = "Location created successfully.";
            public const string LocationAddFailed = "Failed to create location.";
            public const string LocationAlreadyExists = "already exists under the same parent.";
            public const string LocationUpdated = "Location updated successfully.";
            public const string LocationUpdateFailed = "Failed to update location.";
            public const string LocationNotFound = "Location not found.";
            public const string LocationDeleted = "Location deleted successfully.";
            public const string LocationDeleteFailed = "Failed to delete location.";
            public const string LocationsFetched = "Locations retrieved successfully.";
            public const string LocationFetched = "Location retrieved successfully.";
            public const string LocationsFetchFailed = "Failed to retrieve locations.";
            public const string LocationFetchFailed = "Failed to retrieve location.";
        }
        public static class NotificationRuleMessage
        {
            public const string Created = "Notification Rule created successfully.";
            public const string CreateFailled = "Notification Rule failled to create.";
            public const string Updated = "Notification Rule updated successfully.";
            public const string UpdateFailled = "Notification Rule failled to updated.";
            public const string Deleted = "Notification Rule deleted successfully.";
            public const string DeleteFailled = "Notification Rule failled to deleted.";
            public const string NotFound = "Notification Rule not found.";
            public const string FetchedAll = "Records fetched successfully.";
            public const string FetchedSingle = "Record fetched successfully.";
            public const string FetchFailled = "Record failed to retrive.";
        }
        public static class FactoryUserStatusMessage
        {
            public const string UserCreated = "User created successfully.";
            public const string UserCreateFailed = "Failed to create user.";
            public const string UserAlreadyExists = "User with the same email or username already exists.";
            public const string UserUpdated = "User updated successfully.";
            public const string UserUpdateFailed = "Failed to update user.";
            public const string UserNotFound = "User not found.";
            public const string UserDeleted = "User deleted successfully.";
            public const string UserDeleteFailed = "Failed to delete user.";
            public const string UsersFetched = "Users retrieved successfully.";
            public const string UserFetched = "User retrieved successfully.";
            public const string UsersFetchFailed = "Failed to retrieve users.";
            public const string UserFetchFailed = "Failed to retrieve user.";
            public const string UserLimitExceeded = "User limit exceeded. Cannot add more users.";
            public const string InvalidRole = "Invalid RoleId.";
            public const string TenantNotFound = "Tenant settings not found.";
            public const string EmailFailed = "User created but email failed due to server issue.";
            public const string TenantUserNotFound = "Tenant user not found.";
            public const string GlobalUserNotFound = "Global user not found.";
            public const string UserAlreadyDeleted = "User already deleted.";
            public const string UserForceLogout = "Force logout successfully.";
            public const string UserAlreadyForceLoggedOut = "User already force logged out.";
            public const string UserSuspendSuccess = "User suspended successfully.";
            public const string UserUnsuspendSuccess = "User unsuspended successfully.";
            public const string UserSuspendFailed = "Failed to toggle suspension.";
            public const string ManagersFetched = "Managers fetched successfully.";
            public const string UsersExceptManagerFetched = "Users fetched successfully (excluding managers).";
            public const string ErrorDeletingUser = "Error deleting user: ";
            public const string ErrorForceLogoutUser = "Error forcing logout: ";
            public const string ErrorSuspendingUser = "Error toggling suspension: ";
            public const string ErrorFetchingManagers = "Failed to fetch managers: ";
            public const string ErrorFetchingUsersExceptManagers = "Failed to fetch users except managers: ";
        }
        public static class PermissionStatusMessage
        {
            public const string PermissionAdded = "Permission added successfully.";
            public const string PermissionAddFailed = "Failed to add permission.";
            public const string PermissionAlreadyExists = "Permission already exists.";
            public const string PermissionUpdated = "Permission updated successfully.";
            public const string PermissionUpdateFailed = "Failed to update permission.";
            public const string PermissionNotFound = "Permission not found.";
            public const string PermissionDeleted = "Permission deleted successfully.";
            public const string PermissionDeleteFailed = "Failed to delete permission.";
            public const string PermissionsFetched = "Permissions retrieved successfully.";
            public const string PermissionsFetchFailed = "Failed to retrieve permissions.";
            public const string ErrorAddingPermission = "Error adding permission: ";
            public const string ErrorUpdatingPermission = "Error updating permission: ";
            public const string ErrorDeletingPermission = "Error deleting permission: ";
            public const string ErrorFetchingPermissions = "Error fetching permissions: ";
        }
        public static class RolePermissionMappingStatusMessage
        {
            public const string PermissionsAssigned = "Permissions mapped to role successfully.";
            public const string PermissionAssignFailed = "Failed to map permissions to role.";
            public const string PermissionsRemoved = "Permissions removed from role successfully.";
            public const string PermissionRemoveFailed = "Failed to remove permissions from role.";
            public const string PermissionsFetched = "Fetched permissions successfully.";
            public const string PermissionsFetchFailed = "Failed to fetch permissions.";
        }

        public static class RoleStatusMessage
        {
            public const string RoleAdded = "Role added successfully and mapped with permissions.";
            public const string RoleAddFailed = "Failed to add role.";
            public const string RoleAlreadyExists = "Role already exists.";
            public const string RoleUpdated = "Role updated successfully.";
            public const string RoleUpdateFailed = "Failed to update role.";
            public const string RoleNotFound = "Role not found.";
            public const string InvalidPermissions = "Invalid permission IDs provided.";
            public const string RoleDeleted = "Role deleted successfully.";
            public const string RoleDeleteFailed = "Failed to delete role.";
            public const string RolesFetched = "Roles retrieved successfully.";
            public const string RolesFetchFailed = "Failed to fetch roles.";
        }
        public static class SupportFeedbackStatusMessage
        {
            public const string FeedbackAdded = "Feedback submitted successfully.";
            public const string FeedbackUpdated = "Feedback updated successfully.";
            public const string FeedbackAddOrUpdateFailed = "Failed to save feedback.";
            public const string FeedbackTicketNotFound = "Ticket does not exist or is inactive/deleted.";
            public const string FeedbackNotFound = "Feedback not found.";
            public const string FeedbackAcknowledged = "Feedback acknowledged successfully.";
            public const string FeedbackAcknowledgeFailed = "Failed to acknowledge feedback.";
            public const string AcknowledgedByRequired = "AcknowledgedBy cannot be null.";
            public const string FeedbackDeleted = "Feedback deleted successfully.";
            public const string FeedbackDeleteFailed = "Failed to delete feedback.";
            public const string FeedbackFetched = "Feedback retrieved successfully.";
            public const string FeedbackFetchFailed = "Failed to fetch feedback.";
        }
        public static class SupportTicketStatusMessage
        {
            public const string TicketAdded = "Support ticket added successfully.";
            public const string TicketAddFailed = "Failed to add support ticket.";
            public const string TicketUpdated = "Support ticket updated successfully.";
            public const string TicketUpdateFailed = "Failed to update support ticket.";
            public const string TicketDeleted = "Support ticket deleted successfully.";
            public const string TicketDeleteFailed = "Failed to delete support ticket.";
            public const string TicketNotFound = "Support ticket not found.";
            public const string TicketAlreadyExists = "Support ticket already exists.";
            public const string TicketFetched = "Support tickets retrieved successfully.";
            public const string TicketFetchFailed = "Failed to fetch support tickets.";

        }
        public static class AnalyticsAndReportsStatusMessage
        {
            public const string DataFetched = "Analytics and reports fetched successfully.";
            public const string NoRecordsFound = "No records found for the given period and category.";
            public const string FetchFailed = "Failed to fetch analytics and reports.";
            public const string InvalidRequest = "Invalid request parameters for analytics and reports.";
            public const string InternalServerError = "An unexpected error occurred while processing analytics and reports.";
        }
        public static class TeamStatusMessage
        {
            public const string TeamAdded = "Team and members added successfully.";
            public const string TeamAddFailed = "Error adding team.";
            public const string TeamAlreadyExists = "Team already exists.";
            public const string TeamUpdated = "Team and members updated successfully.";
            public const string TeamUpdateFailed = "Error updating team.";
            public const string TeamDeleted = "Team and its members deleted successfully.";
            public const string TeamDeleteFailed = "Error deleting team.";
            public const string TeamFetched = "Teams retrieved successfully.";
            public const string TeamFetchFailed = "Error fetching teams.";
            public const string TeamNotFound = "Team not found.";
            public const string TenantNotFound = "Tenant not found.";
        }
        public static class ServiceRequestStatusMessage
        {
            public const string ServiceRequestCreated = "Service request created successfully.";
            public const string ServiceRequestCreateFailed = "Failed to create service request.";
            public const string ServiceRequestUpdated = "Service request updated successfully.";
            public const string ServiceRequestUpdateFailed = "Failed to update service request.";
            public const string ServiceRequestDeleted = "Service request deleted successfully.";
            public const string ServiceRequestDeleteFailed = "Failed to delete service request.";
            public const string ServiceRequestStatusUpdated = "Service request status updated successfully to.";
            public const string ServiceRequestStatusUpdateFailed = "Failed to update service request status.";
            public const string ServiceRequestFetched = "Service requests retrieved successfully.";
            public const string ServiceRequestFetchFailed = "Failed to retrieve service requests.";
            public const string ServiceRequestNotFound = "Service request not found.";
            public const string ServiceRequestsByStatusFetched = "Service requests with status";
            public const string ServiceRequestsByStatusFetched2 = "retrieved successfully.";
            public const string OverdueServiceRequestsFetched = "Overdue service requests retrieved successfully.";
            public const string OverdueServiceRequestsFetchedFailed = "Failed to retrieve overdue service requests.";
        }

        public static class TechnicianAssignmentStatusMessage
        {
            public const string TechnicianDashboardSummaryNotFound = "Technician Dashboard Summary Data Not Found";
            public const string TechnicianDashboardSummaryFetched = "Technician Dashboard Summary Data retrieved successfully.";
            public const string TechnicianDashboardSummaryFetchedFailed = "Failed to retrieve Technician Dashboard Summary Data.";
            public const string WorkOrdersFetched = "Work orders retrieved successfully.";
            public const string WorkOrdersFetchFailed = "Failed to retrieve work orders.";
            public const string WorkOrderNotFound = "Work order not found.";
            public const string TechniciansFetched = "Technicians retrieved successfully.";
            public const string TechniciansFetchFailed = "Failed to retrieve technicians.";
            public const string TechnicianAssigned = "Work order assigned successfully.";
            public const string TechnicianAssignmentFailed = "Failed to assign technician to work order.";
            public const string TechnicianAssignmentUpdated = "Work order assignment updated successfully.";
            public const string TechnicianAssignmentUpdateFailed = "Failed to update work order assignment.";
            public const string AssignmentHistoryFetched = "Latest assignment history retrieved successfully.";
            public const string AssignmentHistoryFetchFailed = "Failed to retrieve assignment history.";
            public const string NoAssignmentHistoryFound = "No assignment history found.";
        }

        public static class WorkOrderStatusMessage
        {
            public const string WorkOrdersFetchedSuccessfully = "Fetched work orders successfully.";
            public const string WorkOrdersFetchFailed = "Failed to fetch work orders.";
            public const string WorkOrderNotFound = "Work order not found.";
            public const string WorkOrderCreatedSuccessfully = "Work order created successfully.";
            public const string WorkOrderCreationFailed = "Failed to create work order.";
            public const string GenericError = "An error occurred. Please try again later.";
            public const string DatabaseError = "Database error occurred. Please try again.";
            public const string WorkOrderAssignmentFailed = "Failed to assign work order.";
            public const string WorkOrderUpdated = "Work order  updated successfully.";
            public const string WorkOrderUpdateFailed = "Failed to update work order.";
            public const string WorkOrderDeletionFailed = "Failed to delete work order.";
            public const string WorkOrderDeletion = "Work order deleted successfully.";
            public const string LaborAnalyticsFetchedSuccessfully = "Labor & Resource analytics fetched successfully.";
            public const string LaborAnalyticsFetchFailed = "Failed to fetch labor analytics.";
            public const string ResourceUsageAnalyticsFetched = "Resource usage analytics fetched successfully.";
            public const string ResourceUsageAnalyticsFetchFailed = "Failed to fetch resource usage analytics.";
        }
        public static class WorkOrderSubTaskStatusMessage
        {
            public const string WorkOrderSubTaskFetchFailed = "Failed to retrieve work order subtask.";
            public const string WorkOrderSubTasksFetched = "Work order With subtasks retrieved successfully.";
            public const string WorkOrderSubTaskCreated = "Work order subtask created successfully.";
            public const string SubTaskCreated = "Subtask created successfully.";
            public const string WorkOrderSubTaskCreateFailed = "Failed to create work order subtask.";
            public const string WorkOrderNotFound = "Work order not found";
            public const string SubTaskNotFound = "Sub Task not found";
            public const string WorkOrderSubTaskUpdated = "Work order subtask updated successfully.";
            public const string SubTaskUpdated = "Subtask updated successfully.";
            public const string WorkOrderSubTaskUpdateFailed = "Failed to update work order subtask.";
            public const string SubTaskUpdateFailed = "Failed to update subtask.";
            public const string WorkOrderSubTaskDeleted = "Work order subtask deleted successfully.";
            public const string WorkOrderSubTaskDeleteFailed = "Failed to delete work order subtask.";
        }
        public static class AssetDashboardReportStatusMessage
        {
            public const string DataFetched = "Dashboard summary fetched successfully.";
            public const string NoRecordsFound = "No assets found for the given tenant.";
            public const string FetchFailed = "Failed to fetch dashboard summary.";
            public const string InvalidRequest = "Invalid request parameters for asset dashboard report.";
            public const string InternalServerError = "An unexpected error occurred while processing asset dashboard report.";
        }
        public static class AssetDocumentStatusMessage
        {
            public const string DocumentAdded = "Asset document added successfully.";
            public const string DocumentUpdated = "Asset document updated successfully.";
            public const string DocumentDeleted = "Asset document deleted successfully.";
            public const string DocumentsFetched = "Asset documents fetched successfully.";
            public const string TenantNotFound = "Tenant not found.";
            public const string Success = "Success.";

            public const string DuplicateTitle = "Document with the same title already exists for this asset.";
            public const string DocumentNotFound = "Asset document not found.";
            public const string FetchFailed = "Error fetching asset documents.";
            public const string AddFailed = "Error adding asset document.";
            public const string UpdateFailed = "Error updating asset document.";
            public const string DeleteFailed = "Error deleting asset document.";
            public const string InvalidRequest = "Invalid asset document request parameters.";

            public const string FileRequired = "Document file is required.";
            public const string FileSaveFailed = "Error saving document file.";
        }
        public static class AssetLifecycleStatusMessage
        {
            public const string LifecycleCreated = "Asset lifecycle created successfully.";
            public const string LifecycleUpdated = "Asset lifecycle updated successfully.";
            public const string LifecycleDeleted = "Asset lifecycle deleted successfully.";
            public const string LifecyclesFetched = "Asset lifecycles retrieved successfully.";
            public const string LifecycleFetched = "Asset lifecycle retrieved successfully.";
            public const string MetricsFetched = "Asset lifecycle metrics retrieved successfully.";

            public const string LifecycleNotFound = "Asset lifecycle not found.";
            public const string LifecycleNotFoundForAsset = "Asset lifecycle not found for this asset.";
            public const string FetchFailed = "Failed to retrieve asset lifecycles.";
            public const string CreateFailed = "Failed to create asset lifecycle.";
            public const string UpdateFailed = "Failed to update asset lifecycle.";
            public const string DeleteFailed = "Failed to delete asset lifecycle.";

            public const string InvalidStageFilter = "Invalid stage filter value.";
        }
        public static class AssetFinancialAnalysisStatusMessage
        {
            public const string AnalysisCreated = "Financial analysis created successfully.";
            public const string AnalysisDeleted = "Financial analysis deleted successfully.";
            public const string AnalysesFetched = "Financial analyses retrieved successfully.";

            public const string AnalysisNotFound = "Financial analysis not found.";
            public const string CreateFailed = "Failed to create financial analysis.";
            public const string DeleteFailed = "Failed to delete financial analysis.";
            public const string FetchFailed = "Failed to retrieve financial analyses.";
        }
        public static class AssetManagementStatusMessage
        {
            public const string AssetAdded = "Asset and tracking added successfully.";
            public const string AssetUpdated = "Asset and tracking updated successfully.";
            public const string AssetDeleted = "Asset and related records deleted successfully.";
            public const string AssetsFetched = "Assets retrieved successfully.";
            public const string TenantFetched = "Tenant data fetched successfully.";

            public const string DuplicateAsset = "Asset already exists (duplicate Serial Number or Unique ID).";
            public const string DuplicateSerialNumber = "Another asset with the same SerialNumber already exists.";
            public const string DuplicateUniqueId = "Another asset with the same AssetUniqueId already exists.";

            public const string AssetNotFound = "Asset not found.";
            public const string TenantNotFound = "Tenant not found.";

            public const string AddFailed = "Error adding asset.";
            public const string UpdateFailed = "Error updating asset.";
            public const string DeleteFailed = "Error deleting asset.";
            public const string FetchFailed = "Error fetching assets.";
        }
        public static class AssetTrackingStatusMessage
        {
            public const string Created = "Asset tracking created successfully.";
            public const string Updated = "Asset tracking and location updated successfully.";
            public const string Deleted = "Asset tracking deleted successfully.";
            public const string FetchedAll = "Fetched asset trackings successfully.";
            public const string FetchedById = "Fetched asset tracking successfully.";
            public const string FetchedLatest = "Fetched latest asset trackings successfully.";

            public const string NotFound = "Asset tracking not found.";
            public const string NoRecordsFound = "No asset trackings found.";

            public const string CreateFailed = "Failed to create asset tracking.";
            public const string UpdateFailed = "Failed to update asset tracking.";
            public const string DeleteFailed = "Failed to delete asset tracking.";
            public const string FetchFailed = "Failed to fetch asset trackings.";
            public const string FetchByIdFailed = "Failed to fetch asset tracking by ID.";
            public const string FetchLatestFailed = "Failed to fetch latest asset trackings.";
        }
        public static class AssetTypeStatusMessage
        {
            public const string Created = "Asset type created successfully.";
            public const string Updated = "Asset type updated successfully.";
            public const string Deleted = "Asset type deleted successfully.";
            public const string FetchedAll = "Fetched asset types successfully.";
            public const string FetchedById = "Fetched asset type successfully.";

            public const string Duplicate = "Asset type with the same name already exists for this tenant.";
            public const string NotFound = "Asset type not found.";

            public const string CreateFailed = "Failed to create asset type.";
            public const string UpdateFailed = "Failed to update asset type.";
            public const string DeleteFailed = "Failed to delete asset type.";
            public const string FetchFailed = "Failed to fetch asset types.";
            public const string FetchByIdFailed = "Failed to fetch asset type by ID.";
        }
        public static class AuthenticationStatusMessage
        {
            public const string Success = "Success";
            public const string InvalidCredentials = "Invalid credentials";
            public const string AccountSuspended = "Account suspended";
            public const string EmailNotFound = "Email not found in any user table";
            public const string PasswordUpdatedSuperAdmin = "SuperAdmin password updated successfully";
            public const string PasswordUpdatedTenantAdmin = "TenantAdmin password updated successfully";
            public const string PasswordResetSuccess = "Password reset successfully";
            public const string PasswordResetFailed = "Failed to reset password. Please try again.";
            public const string ForbiddenSwitchTenant = "Forbidden - Only SuperAdmin can switch tenants";
            public const string TenantNotFound = "Tenant not found";
            public const string NoTenantAdminFound = "No Tenant Admin found";
        }
        public static class InventoryCostStatusMessage
        {
            public const string CostRetrieved = "Inventory costs retrieved successfully";
            public const string NotFound = "Inventory cost not found";
            public const string Added = "Inventory cost added successfully";
            public const string Updated = "Inventory cost updated successfully";
            public const string Deleted = "Inventory cost deleted successfully";
            public const string Error = "An error occurred while processing inventory costs";
            public const string WorkOrderNotFound = "WorkOrder not found";
            public const string InventoryItemNotFound = "Inventory item not found";
        }
        public static class InventoryStatusMessage
        {
            public const string Created = "Inventory item created successfully.";
            public const string Updated = "Inventory item updated successfully.";
            public const string Deleted = "Inventory item deleted successfully.";
            public const string NotFound = "Inventory item not found.";
            public const string FetchSuccess = "Inventory fetched successfully.";
            public const string CreateFailed = "Failed to create Inventory.";
            public const string UpdateFailed = "Failed to update Inventory.";
            public const string DeleteFailed = "Failed to delete Inventory.";
            public const string FetchFailed = "Failed to fetch Inventory.";
            public const string StockTrackingFetched = "Stock tracking data fetched successfully.";
            public const string StockReservationsFetched = "Stock reservations data fetched successfully.";
            public const string StockTrackingFetchFailed = "Stock tracking data fetch failed.";
            public const string StockReservationsFetchFailed = "Stock reservations data fetch failed.";
            public const string SerialBatchFetched = "Serial/Batch tracking data fetched successfully.";
            public const string SerialBatchFetchFailed = "Serial/Batch tracking data fetch failed.";
            public const string EmailFailed = "Inventory updated but email notification failed.";
        }
        public static class TransactionStatusMessage
        {
            public const string Created = "Transaction created successfully";
            public const string Updated = "Transaction updated successfully";
            public const string Deleted = "Transaction deleted successfully";
            public const string NotFound = "Transaction not found";
            public const string FetchSuccess = "Transaction(s) retrieved successfully";
            public const string FetchFailed = "Failed to fetch transaction(s)";
            public const string StatusUpdated = "Transaction status updated successfully";
            public const string CreateFailed = "Failed to create transaction";
            public const string UpdateFailed = "Failed to update transaction";
            public const string DeleteFailed = "Failed to delete transaction";
            public const string StatusUpdateFailed = "Failed to update transaction status";
            public const string AllTransactionsFetched = "All transactions retrieved successfully";
            public const string DateRangeTransactionsFetched = "Date range transactions retrieved successfully";
            public const string PartTransactionsFetched = "Part transactions retrieved successfully";
        }
        public static class PurchaseRequisitionStatusMessage
        {
            public const string FetchAllSuccess = "Purchase requisitions retrieved successfully";
            public const string FetchByIdSuccess = "Purchase requisition retrieved successfully";
            public const string NotFound = "Purchase requisition not found";
            public const string FetchAllFailed = "Error retrieving purchase requisitions";
            public const string FetchByIdFailed = "Error retrieving purchase requisition";
        }
        public static class AlertRuleStatusMessage
        {
            public const string Added = "Alert rule created successfully";
            public const string Updated = "Alert rule updated successfully";
            public const string Deleted = "Alert rule deleted successfully";
            public const string FetchAllSuccess = "Alert rules retrieved successfully";
            public const string FetchByIdSuccess = "Alert rule retrieved successfully";
            public const string NotFound = "Alert rule not found";
            public const string AddFailed = "Error adding alert rule";
            public const string UpdateFailed = "Error updating alert rule";
            public const string DeleteFailed = "Error deleting alert rule";
            public const string FetchAllFailed = "Error retrieving alert rules";
            public const string FetchByIdFailed = "Error retrieving alert rule";
        }
        public static class DeviceConfigurationStatusMessage
        {
            public const string AddSuccess = "Device configuration added successfully";
            public const string UpdateSuccess = "Device configuration updated successfully";
            public const string FetchAllSuccess = "Device configurations retrieved successfully";
            public const string FetchByIdSuccess = "Device configuration retrieved successfully";
            public const string NotFound = "Device configuration not found";
            public const string InvalidRequest = "Invalid device configuration request";
            public const string AddFailed = "Error adding device configuration";
            public const string UpdateFailed = "Error updating device configuration";
            public const string FetchAllFailed = "Error retrieving device configurations";
            public const string FetchByIdFailed = "Error retrieving device configuration";
        }
        public static class FactoryDeviceStatusMessage
        {
            public const string AddSuccess = "Device created successfully";
            public const string UpdateSuccess = "Device updated successfully";
            public const string DeleteSuccess = "Device deleted successfully";
            public const string FetchAllSuccess = "Devices retrieved successfully";
            public const string FetchByIdSuccess = "Device retrieved successfully";
            public const string AlreadyExists = "Device with the same code already exists";
            public const string DuplicateCode = "Device code already exists for this tenant";
            public const string NotFound = "Device not found";
            public const string AddFailed = "Error creating device";
            public const string UpdateFailed = "Error updating device";
            public const string DeleteFailed = "Error deleting device";
            public const string FetchAllFailed = "Error retrieving devices";
            public const string FetchByIdFailed = "Error retrieving device";
        }
        public static class MqttTopicStatusMessage
        {
            public const string AddSuccess = "MQTT Topic created successfully";
            public const string UpdateSuccess = "MQTT Topic updated successfully";
            public const string DeleteSuccess = "MQTT Topic deleted successfully";
            public const string FetchAllSuccess = "Topics retrieved successfully";
            public const string FetchByIdSuccess = "Topic retrieved successfully";
            public const string NotFound = "MQTT Topic not found";
            public const string NoTopics = "No topics found";
            public const string AddFailed = "Error creating MQTT Topic";
            public const string UpdateFailed = "Error updating MQTT Topic";
            public const string DeleteFailed = "Error deleting MQTT Topic";
            public const string FetchAllFailed = "Error retrieving topics";
            public const string FetchByIdFailed = "Error retrieving topic";
        }
        public static class ReorderRuleStatusMessage
        {
            public const string CreateSuccess = "Reorder rule created successfully";
            public const string UpdateSuccess = "Reorder rule updated successfully";
            public const string DeleteSuccess = "Reorder rule deleted successfully";
            public const string FetchAllSuccess = "Reorder rules retrieved successfully";
            public const string FetchByIdSuccess = "Reorder rule retrieved successfully";
            public const string DashboardSuccess = "Dashboard data retrieved successfully";
            public const string InventoryNotFound = "Inventory item not found";
            public const string SupplierNotFound = "Supplier not found or inactive";
            public const string RuleNotFound = "Reorder rule not found";
            public const string CreateFailed = "Error creating reorder rule";
            public const string UpdateFailed = "Error updating reorder rule";
            public const string DeleteFailed = "Error deleting reorder rule";
            public const string FetchAllFailed = "Error retrieving reorder rules";
            public const string FetchByIdFailed = "Error retrieving reorder rule";
            public const string DashboardFailed = "Error retrieving dashboard data";
        }
        public static class SupplierManagementStatusMessage
        {
            public const string CreateSuccess = "Supplier created successfully";
            public const string UpdateSuccess = "Supplier updated successfully";
            public const string DeleteSuccess = "Supplier deleted successfully";
            public const string FetchAllSuccess = "Suppliers retrieved successfully";
            public const string FetchByIdSuccess = "Supplier retrieved successfully";
            public const string SupplierNotFound = "Supplier not found";
            public const string CreateFailed = "Error creating supplier";
            public const string UpdateFailed = "Error updating supplier";
            public const string DeleteFailed = "Error deleting supplier";
            public const string FetchAllFailed = "Error retrieving suppliers";
            public const string FetchByIdFailed = "Error retrieving supplier";
        }
        public static class TelemetryStatusMessage
        {
            public const string AddSuccess = "Telemetry added successfully";
            public const string FetchTelemetrySuccess = "Telemetry retrieved successfully";
            public const string FetchStatusLogsSuccess = "Device status logs retrieved successfully";
            public const string DeviceNotFound = "Device not found";
            public const string AddFailed = "Error adding telemetry";
            public const string FetchTelemetryFailed = "Error retrieving telemetry";
            public const string FetchStatusLogsFailed = "Error retrieving device status logs";
        }
        public static class NotificationStatusMessage
        {
            public const string FetchAllSuccess = "Notifications retrieved successfully";
            public const string FetchWorkOrderSuccess = "Work order notifications retrieved successfully";
            public const string FetchUnreadSuccess = "Unread notifications retrieved successfully";
            public const string FetchUserSuccess = "User notifications retrieved successfully";
            public const string MarkAsReadSuccess = "Notification marked as read";
            public const string NotificationNotFound = "Notification not found";
            public const string FetchFailed = "Error retrieving notifications";
            public const string MarkAsReadFailed = "Error marking notification as read";
        }
        public static class AlertNotificationStatusMessage
        {
            public const string CreateSuccess = "Alert Notification created successfully";
            public const string UpdateSuccess = "Alert Notification updated successfully";
            public const string DeleteSuccess = "Alert Notification deleted successfully";
            public const string FetchAllSuccess = "Alert Notifications retrieved successfully";
            public const string FetchByIdSuccess = "Alert Notification retrieved successfully";
            public const string NotFound = "Alert Notification not found";
            public const string CreateFailed = "Error creating Alert Notification";
            public const string UpdateFailed = "Error updating Alert Notification";
            public const string DeleteFailed = "Error deleting Alert Notification";
            public const string FetchAllFailed = "Error retrieving all Alert Notifications";
            public const string FetchByIdFailed = "Error retrieving Alert Notification by ID";
        }
        public static class MaintenanceHistoryStatusMessage
        {
            public const string CreateSuccess = "Maintenance record created successfully";
            public const string UpdateSuccess = "Maintenance record updated successfully";
            public const string DeleteSuccess = "Maintenance record deleted successfully";
            public const string FetchAllSuccess = "Maintenance records retrieved successfully";
            public const string FetchByIdSuccess = "Maintenance record retrieved successfully";
            public const string FetchByAssetSuccess = "Maintenance records for asset retrieved successfully";
            public const string MetricsFetchSuccess = "Maintenance metrics retrieved successfully";
            public const string NotFound = "Maintenance record not found";
            public const string AssetRecordNotFound = "No maintenance records found for this asset";
            public const string CreateFailed = "Error creating maintenance record";
            public const string UpdateFailed = "Error updating maintenance record";
            public const string DeleteFailed = "Error deleting maintenance record";
            public const string FetchAllFailed = "Error retrieving maintenance records";
            public const string FetchByIdFailed = "Error retrieving maintenance record";
            public const string FetchByAssetFailed = "Error retrieving maintenance records for asset";
            public const string MetricsFetchFailed = "Error retrieving maintenance metrics";
        }
        public static class MaintenanceScheduleStatusMessage
        {
            public const string CreateSuccess = "Maintenance schedule and occurrences created successfully";
            public const string UpdateSuccess = "Maintenance schedule and linked WorkOrder updated successfully";
            public const string DeleteSuccess = "Maintenance schedule, occurrences, and linked WorkOrders soft deleted successfully";
            public const string ApproveSuccess = "Maintenance schedule approved successfully";
            public const string RejectSuccess = "Maintenance schedule rejected successfully";
            public const string FetchAllSuccess = "Maintenance schedules with occurrences retrieved successfully";
            public const string FetchByIdSuccess = "Maintenance schedule retrieved successfully";
            public const string FetchOccurrencesSuccess = "Occurrences retrieved successfully";
            public const string UpcomingOccurrencesSuccess = "Upcoming occurrences retrieved successfully";
            public const string RegenerateSuccess = "Occurrences regenerated successfully";
            public const string NextDueRecalculatedSuccess = "Next due date recalculated successfully";
            public const string NotFound = "Maintenance schedule not found";
            public const string ScheduleNotFound = "Schedule not found";
            public const string CreateFailed = "Error creating maintenance schedule";
            public const string UpdateFailed = "Error updating maintenance schedule";
            public const string DeleteFailed = "Error deleting maintenance schedule";
            public const string ApproveFailed = "Error approving maintenance schedule";
            public const string RejectFailed = "Error rejecting maintenance schedule";
            public const string FetchAllFailed = "Error retrieving maintenance schedules";
            public const string FetchByIdFailed = "Error retrieving maintenance schedule by ID";
            public const string FetchOccurrencesFailed = "Error retrieving schedule occurrences";
            public const string UpcomingOccurrencesFailed = "Error retrieving upcoming occurrences";
            public const string RegenerateFailed = "Error regenerating occurrences";
            public const string NextDueRecalculatedFailed = "Error recalculating next due date";
        }
        public static class MaintenanceTaskStatusMessage
        {
            public const string CreateSuccess = "Maintenance task created successfully";
            public const string UpdateSuccess = "Maintenance task updated successfully";
            public const string DeleteSuccess = "Maintenance task deleted successfully";
            public const string VerifySuccess = "Task verified successfully";
            public const string FetchByIdSuccess = "Maintenance task retrieved successfully";
            public const string FetchAllSuccess = "Maintenance tasks retrieved successfully";
            public const string FetchByWorkOrderSuccess = "Maintenance tasks retrieved successfully for work order";
            public const string FetchByStatusSuccess = "Maintenance tasks retrieved successfully by status";
            public const string TasksRequiringVerificationSuccess = "Tasks requiring verification retrieved successfully";
            public const string UpdateTaskStatusSuccess = "Task status updated successfully";
            public const string NotFound = "Maintenance task not found";
            public const string TaskNotFound = "Task not found or doesn't require verification";
            public const string MustBeCompletedBeforeVerification = "Task must be completed before verification";
            public const string CreateFailed = "Error creating maintenance task";
            public const string UpdateFailed = "Error updating maintenance task";
            public const string DeleteFailed = "Error deleting maintenance task";
            public const string VerifyFailed = "Error verifying maintenance task";
            public const string FetchByIdFailed = "Error retrieving maintenance task by ID";
            public const string FetchAllFailed = "Error retrieving maintenance tasks";
            public const string FetchByWorkOrderFailed = "Error retrieving maintenance tasks by work order";
            public const string FetchByStatusFailed = "Error retrieving maintenance tasks by status";
            public const string TasksRequiringVerificationFailed = "Error retrieving tasks requiring verification";
            public const string UpdateTaskStatusFailed = "Error updating task status";
        }

        public static class BadgeStatusMessage
        {
            public const string BadgesFetched = "Badges retrieved successfully.";
            public const string BadgeCreated = "Badge created successfully.";
            public const string BadgeUpdated = "Badge updated successfully.";
            public const string BadgeDeleted = "Badge deleted successfully.";
            public const string BadgeActivated = "Badge activated successfully.";
            public const string DuplicateBadgeName = "Badge with the same name already exists.";
            public const string BadgeNotFound = "Badge not found.";
            public const string Success = "Success";
        }

        public static class TeamAlertNotificationStatusMessage
        {
            public const string TeamAlertNotificationAdded = "Team alert notification created successfully";
            public const string TeamAlertNotificationAddFailed = "Failed to create team alert notification";
            public const string TeamAlertNotificationUpdated = "Team alert notification updated successfully";
            public const string TeamAlertNotificationUpdateFailed = "Failed to update team alert notification";
            public const string TeamAlertNotificationDeleted = "Team alert notification deleted successfully";
            public const string TeamAlertNotificationDeleteFailed = "Failed to delete team alert notification";
            public const string TeamAlertNotificationsFetched = "Team alert notifications fetched successfully";
            public const string TeamAlertNotificationsFetchFailed = "Failed to fetch team alert notifications";
            public const string TeamAlertNotificationNotFound = "Team alert notification not found";
            public const string TeamAlertNotificationMarkedAsRead = "Notification marked as read successfully";
            public const string TeamAlertNotificationMarkAsReadFailed = "Failed to mark notification as read";
        }

    }
}
