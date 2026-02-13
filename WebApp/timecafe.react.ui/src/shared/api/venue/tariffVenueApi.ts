import {httpClient} from "@api/httpClient.ts";
import type {BillingType, Tariff} from "@app-types/tariff.ts";
import {normalizeUnknownError} from "@api/errors/normalize.ts";
import type {TariffWithTheme} from "@app-types/tariffWithTheme.ts";

type ApiBillingType = 1 | 2;

export interface GetTariffsPageRequest {
    pageNumber: number;
    pageSize: number;
}

export interface GetTariffsResponse {
    tariffs: TariffWithThemeApiResponse[];
}

export interface GetTariffResponse {
    tariff: TariffApiResponse;
}

export interface GetTariffsPageResponse {
    tariffs: TariffWithThemeApiResponse[];
    totalCount: number;
}

export interface TariffWithThemeApiResponse {
    tariffId: string;
    name: string;
    description?: string | null;
    pricePerMinute: number;
    billingType: ApiBillingType;
    isActive: boolean;
    createdAt: string;
    lastModified: string;
    themeId?: string | null;
    themeName: string;
    themeEmoji?: string | null;
    themeColors?: string | null;
}

export interface TariffApiResponse {
    tariffId: string;
    name: string;
    description?: string | null;
    pricePerMinute: number;
    billingType: ApiBillingType;
    themeId?: string | null;
    isActive: boolean;
}

export interface CreateTariffRequest {
    name: string;
    description: string | null;
    pricePerMinute: number;
    billingType: BillingType;
    themeId: string | null;
    isActive: boolean;
}

export interface UpdateTariffRequest {
    tariffId: string;
    name: string;
    description: string | null;
    pricePerMinute: number;
    billingType: BillingType;
    themeId: string | null;
    isActive: boolean;
}

export interface CreateTariffResponse {
    message: string;
    tariff: Tariff;
}

export interface UpdateTariffResponse {
    message: string;
    tariff: Tariff;
}

export interface TariffCommandMessageResponse {
    message: string;
}


const mapTariff = (item: TariffApiResponse): Tariff => ({
    tariffId: item.tariffId,
    name: item.name,
    description: item.description ?? "",
    billingType: item.billingType,
    pricePerMinute: item.pricePerMinute,
    isActive: item.isActive,
});

export class TariffVenueApi {
    static async getActiveTariffs(): Promise<TariffWithTheme[]> {
        try {
            const res = await httpClient.get<GetTariffsResponse>(`/venue/tariffs/active`);
            return res.data.tariffs;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getAllTariffs(): Promise<TariffWithTheme[]> {
        try {
            const res = await httpClient.get<GetTariffsResponse>(`/venue/tariffs`);
            return res.data.tariffs
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getTariffById(tariffId: string): Promise<Tariff> {
        try {
            const res = await httpClient.get<GetTariffResponse>(`/venue/tariffs/${tariffId}`);
            return mapTariff(res.data.tariff);
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getTariffsByBillingType(billingType: BillingType): Promise<TariffWithTheme[]> {
        try {
            const res = await httpClient.get<GetTariffsResponse>(`/venue/tariffs/billing-type/${billingType}`);
            return res.data.tariffs;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getTariffsPage(request: GetTariffsPageRequest): Promise<{
        tariffs: TariffWithTheme[];
        totalCount: number
    }> {
        try {
            const res = await httpClient.get<GetTariffsPageResponse>("/venue/tariffs/page", {
                params: request
            });
            return {
                tariffs: res.data.tariffs,
                totalCount: res.data.totalCount,
            };
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }


    static async createTariff(request: CreateTariffRequest): Promise<CreateTariffResponse> {
        try {
            const payload = {
                name: request.name,
                description: request.description,
                pricePerMinute: request.pricePerMinute,
                billingType: request.billingType,
                themeId: request.themeId,
                isActive: request.isActive,
            };
            const res = await httpClient.post<{ message: string; tariff: TariffApiResponse }>(`/venue/tariffs`, payload);
            return {
                message: res.data.message,
                tariff: mapTariff(res.data.tariff),
            };
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async updateTariff(request: UpdateTariffRequest): Promise<UpdateTariffResponse> {
        try {
            const payload = {
                tariffId: request.tariffId,
                name: request.name,
                description: request.description,
                pricePerMinute: request.pricePerMinute,
                billingType: request.billingType,
                themeId: request.themeId,
                isActive: request.isActive,
            };
            const res = await httpClient.put<{ message: string; tariff: TariffApiResponse }>(`/venue/tariffs`, payload);
            return {
                message: res.data.message,
                tariff: mapTariff(res.data.tariff),
            };
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async deleteTariff(tariffId: string): Promise<TariffCommandMessageResponse> {
        try {
            const res = await httpClient.delete<TariffCommandMessageResponse>(`/venue/tariffs/${tariffId}`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async activateTariff(tariffId: string): Promise<TariffCommandMessageResponse> {
        try {
            const res = await httpClient.post<TariffCommandMessageResponse>(`/venue/tariffs/${tariffId}/activate`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async deactivateTariff(tariffId: string): Promise<TariffCommandMessageResponse> {
        try {
            const res = await httpClient.post<TariffCommandMessageResponse>(`/venue/tariffs/${tariffId}/deactivate`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

}
