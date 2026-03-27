import type {RootState} from "@store";

export const selectBalance = (state: RootState) => state.billing.balance;
export const selectBalanceRub = (state: RootState) => state.billing.balanceRub;
export const selectDebtRub = (state: RootState) => state.billing.debtRub;
export const selectTransactions = (state: RootState) => state.billing.transactions;
export const selectBillingLoading = (state: RootState) => state.billing.loading;
export const selectLoadingTransactions = (state: RootState) => state.billing.loadingTransactions;
export const selectBillingError = (state: RootState) => state.billing.error;
export const selectCheckoutError = (state: RootState) => state.billing.checkoutError;
export const selectCheckoutUrl = (state: RootState) => state.billing.checkoutUrl;
export const selectInitializingCheckout = (state: RootState) => state.billing.initializingCheckout;
export const selectBillingPagination = (state: RootState) => state.billing.pagination;
