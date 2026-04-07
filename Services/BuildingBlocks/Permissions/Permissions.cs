namespace BuildingBlocks.Permissions;

public static class Permissions
{
    public const string UserProfileProfileCreate = "userprofile.profile.create";
    public const string UserProfileProfileRead = "userprofile.profile.read";
    public const string UserProfileProfileUpdate = "userprofile.profile.update";
    public const string UserProfileProfileDelete = "userprofile.profile.delete";
    public const string UserProfileAdditionalInfoCreate = "userprofile.additionalinfo.create";
    public const string UserProfileAdditionalInfoRead = "userprofile.additionalinfo.read";
    public const string UserProfileAdditionalInfoUpdate = "userprofile.additionalinfo.update";
    public const string UserProfileAdditionalInfoDelete = "userprofile.additionalinfo.delete";
    public const string UserProfilePhotoCreate = "userprofile.photo.create";
    public const string UserProfilePhotoRead = "userprofile.photo.read";
    public const string UserProfilePhotoDelete = "userprofile.photo.delete";

    public const string BillingBalanceRead = "billing.balance.read";
    public const string BillingDebtRead = "billing.debt.read";
    public const string BillingTransactionCreate = "billing.transaction.create";
    public const string BillingTransactionRead = "billing.transaction.read";
    public const string BillingPaymentInitialize = "billing.payment.initialize";
    public const string BillingPaymentHistoryRead = "billing.payment.read";

    public const string VenueTariffCreate = "venue.tariff.create";
    public const string VenueTariffRead = "venue.tariff.read";
    public const string VenueTariffUpdate = "venue.tariff.update";
    public const string VenueTariffDelete = "venue.tariff.delete";
    public const string VenueTariffActivate = "venue.tariff.activate";
    public const string VenueTariffDeactivate = "venue.tariff.deactivate";
    public const string VenuePromotionCreate = "venue.promotion.create";
    public const string VenuePromotionRead = "venue.promotion.read";
    public const string VenuePromotionUpdate = "venue.promotion.update";
    public const string VenuePromotionDelete = "venue.promotion.delete";
    public const string VenuePromotionActivate = "venue.promotion.activate";
    public const string VenuePromotionDeactivate = "venue.promotion.deactivate";
    public const string VenueThemeCreate = "venue.theme.create";
    public const string VenueThemeRead = "venue.theme.read";
    public const string VenueThemeUpdate = "venue.theme.update";
    public const string VenueThemeDelete = "venue.theme.delete";
    public const string VenueVisitCreate = "venue.visit.create";
    public const string VenueVisitRead = "venue.visit.read";
    public const string VenueVisitUpdate = "venue.visit.update";
    public const string VenueVisitDelete = "venue.visit.delete";
    public const string VenueVisitEnd = "venue.visit.end";

    public const string AccountSelfRead = "auth.account.self.read";
    public const string AccountAdminRead = "auth.account.admin.read";
    public const string AccountEmailChange = "auth.account.email.change";
    public const string AccountPasswordChange = "auth.account.password.change";
    public const string AccountPhoneSave = "auth.account.phone.save";
    public const string AccountPhoneClear = "auth.account.phone.clear";
    public const string AccountPhoneGenerate = "auth.account.phone.generate";
    public const string AccountPhoneVerify = "auth.account.phone.verify";
    public const string AccountPhoneStatusRead = "auth.account.phone.status.read";

    public const string DebugRateLimitRead = "auth.debug.ratelimit.read";
    public const string DebugProtectedRead = "auth.debug.protected.read";

    public const string RbacRoleCreate = "auth.rbac.role.create";
    public const string RbacRoleRead = "auth.rbac.role.read";
    public const string RbacRoleUpdate = "auth.rbac.role.update";
    public const string RbacRoleDelete = "auth.rbac.role.delete";
    public const string RbacRoleClaimsUpdate = "auth.rbac.role.claims.update";
    public const string RbacPermissionRead = "auth.rbac.permission.read";
    public const string RbacUserRoleAssign = "auth.rbac.userrole.assign";
    public const string RbacUserRoleRemove = "auth.rbac.userrole.remove";
}
