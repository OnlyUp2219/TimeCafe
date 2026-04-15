import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {Tariff} from "@app-types/tariff";
import type {Visit} from "@app-types/visit";
import type {VisitWithTariff} from "@app-types/visitWithTariff";
import type {TariffWithTheme} from "@app-types/tariffWithTheme";

interface GetTariffsResponse {
    tariffs: TariffWithTheme[];
}

interface GetTariffResponse {
    tariff: TariffApiResponse;
}

interface TariffApiResponse {
    tariffId: string;
    name: string;
    description?: string | null;
    pricePerMinute: number;
    billingType: 1 | 2;
    themeId?: string | null;
    isActive: boolean;
}

interface VisitsResponse {
    visits: VisitWithTariff[];
}

interface HasActiveVisitResponse {
    hasActiveVisit: boolean;
}

export interface CreateVisitRequest {
    userId: string;
    tariffId: string;
    plannedMinutes?: number;
    requirePositiveBalance?: boolean;
    requireEnoughForPlanned?: boolean;
}

interface CreateVisitResponse {
    message: string;
    visit: Visit;
}

interface EndVisitResponse {
    message: string;
    visit: Visit;
    calculatedCost: number;
}

const mapTariff = (item: TariffApiResponse): Tariff => ({
    tariffId: item.tariffId,
    name: item.name,
    description: item.description ?? "",
    billingType: item.billingType,
    pricePerMinute: item.pricePerMinute,
    isActive: item.isActive,
});

interface GetTariffsPageResponse {
    tariffs: TariffWithTheme[];
    totalCount: number;
}

interface GetTariffsPageArgs {
    pageNumber: number;
    pageSize: number;
}

interface GetVisitsPageResponse {
    visits: VisitWithTariff[];
    totalCount: number;
}

interface GetVisitsPageArgs {
    pageNumber: number;
    pageSize: number;
}

interface CreateTariffRequest {
    name: string;
    description?: string;
    pricePerMinute: number;
    billingType: 1 | 2;
    themeId?: string;
    isActive: boolean;
}

interface UpdateTariffRequest {
    tariffId: string;
    name: string;
    description?: string;
    pricePerMinute: number;
    billingType: 1 | 2;
    themeId?: string;
    isActive: boolean;
}

export interface Promotion {
    promotionId: string;
    name: string;
    description?: string;
    discountPercent?: number;
    validFrom: string;
    validTo: string;
    isActive: boolean;
    createdAt: string;
}

export interface CreatePromotionRequest {
    name: string;
    description?: string;
    discountPercent?: number;
    validFrom: string;
    validTo: string;
    isActive: boolean;
}

export interface UpdatePromotionRequest {
    promotionId: string;
    name: string;
    description?: string;
    discountPercent?: number;
    validFrom: string;
    validTo: string;
    isActive: boolean;
}

export interface Theme {
    themeId: string;
    name: string;
    emoji?: string;
    colors?: string;
}

export interface CreateThemeRequest {
    name: string;
    emoji?: string;
    colors?: string;
}

export interface UpdateThemeRequest {
    themeId: string;
    name: string;
    emoji?: string;
    colors?: string;
}

export const venueApi = createApi({
    reducerPath: "venueApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["ActiveTariffs", "Tariff", "AllTariffs", "ActiveVisit", "VisitHistory", "VisitsPage", "Promotions", "Themes"],
    endpoints: (builder) => ({
        getActiveTariffs: builder.query<TariffWithTheme[], void>({
            query: () => "/venue/tariffs/active",
            transformResponse: (response: GetTariffsResponse) => response.tariffs,
            providesTags: ["ActiveTariffs"],
        }),

        getAllTariffs: builder.query<TariffWithTheme[], void>({
            query: () => "/venue/tariffs",
            transformResponse: (response: GetTariffsResponse) => response.tariffs,
            providesTags: ["AllTariffs"],
        }),

        getTariffsPage: builder.query<GetTariffsPageResponse, GetTariffsPageArgs>({
            query: ({pageNumber, pageSize}) => ({
                url: "/venue/tariffs/page",
                params: {pageNumber, pageSize},
            }),
            providesTags: ["AllTariffs"],
            keepUnusedDataFor: 0,
        }),

        getTariffById: builder.query<Tariff, string>({
            query: (tariffId) => `/venue/tariffs/${tariffId}`,
            transformResponse: (response: GetTariffResponse) => mapTariff(response.tariff),
            providesTags: (_result, _error, tariffId) => [{type: "Tariff", id: tariffId}],
        }),

        hasActiveVisit: builder.query<boolean, string>({
            query: (userId) => `/venue/visits/has-active/${userId}`,
            transformResponse: (response: HasActiveVisitResponse) => response.hasActiveVisit,
            providesTags: (_result, _error, userId) => [{type: "ActiveVisit", id: userId}],
        }),

        getActiveVisitByUser: builder.query<VisitWithTariff, string>({
            query: (userId) => `/venue/visits/active/${userId}`,
            transformResponse: (response: { visit: VisitWithTariff }) => response.visit,
            providesTags: (_result, _error, userId) => [{type: "ActiveVisit", id: userId}],
        }),

        getVisitHistory: builder.query<VisitWithTariff[], string>({
            query: (userId) => `/venue/visits/history/${userId}`,
            transformResponse: (response: VisitsResponse) => response.visits,
            providesTags: (_result, _error, userId) => [{type: "VisitHistory", id: userId}],
        }),

        getVisitsPage: builder.query<GetVisitsPageResponse, GetVisitsPageArgs>({
            query: ({pageNumber, pageSize}) => ({
                url: "/venue/visits/page",
                params: {pageNumber, pageSize},
            }),
            providesTags: ["VisitsPage"],
        }),

        createVisit: builder.mutation<CreateVisitResponse, CreateVisitRequest>({
            query: (body) => ({
                url: "/venue/visits",
                method: "POST",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => [
                {type: "ActiveVisit", id: arg.userId},
                {type: "VisitHistory", id: arg.userId},
            ],
        }),

        endVisit: builder.mutation<EndVisitResponse, string>({
            query: (visitId) => ({
                url: `/venue/visits/${visitId}/end`,
                method: "POST",
            }),
            invalidatesTags: ["ActiveVisit", "VisitHistory"],
        }),

        createTariff: builder.mutation<{message: string; tariff: TariffWithTheme}, CreateTariffRequest>({
            query: (body) => ({
                url: "/venue/tariffs",
                method: "POST",
                body,
            }),
            invalidatesTags: ["AllTariffs", "ActiveTariffs"],
        }),

        updateTariff: builder.mutation<{message: string; tariff: TariffWithTheme}, UpdateTariffRequest>({
            query: ({tariffId, ...body}) => ({
                url: `/venue/tariffs/${tariffId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["AllTariffs", "ActiveTariffs"],
        }),

        deleteTariff: builder.mutation<{message: string}, string>({
            query: (tariffId) => ({
                url: `/venue/tariffs/${tariffId}`,
                method: "DELETE",
            }),
            invalidatesTags: ["AllTariffs", "ActiveTariffs"],
        }),

        activateTariff: builder.mutation<{message: string}, string>({
            query: (tariffId) => ({
                url: `/venue/tariffs/${tariffId}/activate`,
                method: "POST",
            }),
            invalidatesTags: ["AllTariffs", "ActiveTariffs"],
        }),

        deactivateTariff: builder.mutation<{message: string}, string>({
            query: (tariffId) => ({
                url: `/venue/tariffs/${tariffId}/deactivate`,
                method: "POST",
            }),
            invalidatesTags: ["AllTariffs", "ActiveTariffs"],
        }),

        getAllPromotions: builder.query<Promotion[], void>({
            query: () => "/venue/promotions",
            transformResponse: (response: {promotions: Promotion[]}) => response.promotions,
            providesTags: ["Promotions"],
        }),

        createPromotion: builder.mutation<{message: string; promotion: Promotion}, CreatePromotionRequest>({
            query: (body) => ({
                url: "/venue/promotions",
                method: "POST",
                body,
            }),
            invalidatesTags: ["Promotions"],
        }),

        updatePromotion: builder.mutation<{message: string; promotion: Promotion}, UpdatePromotionRequest>({
            query: ({promotionId, ...body}) => ({
                url: `/venue/promotions/${promotionId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["Promotions"],
        }),

        deletePromotion: builder.mutation<{message: string}, string>({
            query: (promotionId) => ({
                url: `/venue/promotions/${promotionId}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Promotions"],
        }),

        activatePromotion: builder.mutation<{message: string}, string>({
            query: (promotionId) => ({
                url: `/venue/promotions/${promotionId}/activate`,
                method: "POST",
            }),
            invalidatesTags: ["Promotions"],
        }),

        deactivatePromotion: builder.mutation<{message: string}, string>({
            query: (promotionId) => ({
                url: `/venue/promotions/${promotionId}/deactivate`,
                method: "POST",
            }),
            invalidatesTags: ["Promotions"],
        }),

        getAllThemes: builder.query<Theme[], void>({
            query: () => "/venue/themes",
            transformResponse: (response: {themes: Theme[]}) => response.themes,
            providesTags: ["Themes"],
        }),

        createTheme: builder.mutation<{message: string; theme: Theme}, CreateThemeRequest>({
            query: (body) => ({
                url: "/venue/themes",
                method: "POST",
                body,
            }),
            invalidatesTags: ["Themes"],
        }),

        updateTheme: builder.mutation<{message: string; theme: Theme}, UpdateThemeRequest>({
            query: ({themeId, ...body}) => ({
                url: `/venue/themes/${themeId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: ["Themes"],
        }),

        deleteTheme: builder.mutation<{message: string}, string>({
            query: (themeId) => ({
                url: `/venue/themes/${themeId}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Themes"],
        }),
    }),
});

export const {
    useGetActiveTariffsQuery,
    useGetAllTariffsQuery,
    useGetTariffsPageQuery,
    useGetTariffByIdQuery,
    useHasActiveVisitQuery,
    useGetActiveVisitByUserQuery,
    useLazyGetActiveVisitByUserQuery,
    useGetVisitHistoryQuery,
    useGetVisitsPageQuery,
    useCreateVisitMutation,
    useEndVisitMutation,
    useCreateTariffMutation,
    useUpdateTariffMutation,
    useDeleteTariffMutation,
    useActivateTariffMutation,
    useDeactivateTariffMutation,
    useGetAllPromotionsQuery,
    useCreatePromotionMutation,
    useUpdatePromotionMutation,
    useDeletePromotionMutation,
    useActivatePromotionMutation,
    useDeactivatePromotionMutation,
    useGetAllThemesQuery,
    useCreateThemeMutation,
    useUpdateThemeMutation,
    useDeleteThemeMutation,
} = venueApi;
