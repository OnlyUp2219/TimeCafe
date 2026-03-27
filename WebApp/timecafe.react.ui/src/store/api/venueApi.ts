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

export const venueApi = createApi({
    reducerPath: "venueApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["ActiveTariffs", "Tariff", "ActiveVisit", "VisitHistory"],
    endpoints: (builder) => ({
        getActiveTariffs: builder.query<TariffWithTheme[], void>({
            query: () => "/venue/tariffs/active",
            transformResponse: (response: GetTariffsResponse) => response.tariffs,
            providesTags: ["ActiveTariffs"],
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
                url: "/venue/visits/end",
                method: "POST",
                body: {visitId},
            }),
            invalidatesTags: ["ActiveVisit", "VisitHistory"],
        }),
    }),
});

export const {
    useGetActiveTariffsQuery,
    useGetTariffByIdQuery,
    useHasActiveVisitQuery,
    useGetActiveVisitByUserQuery,
    useLazyGetActiveVisitByUserQuery,
    useGetVisitHistoryQuery,
    useCreateVisitMutation,
    useEndVisitMutation,
} = venueApi;
