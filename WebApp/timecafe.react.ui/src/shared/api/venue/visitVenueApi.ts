import type {Visit} from "@app-types/visit.ts";
import {normalizeUnknownError} from "@api/errors/normalize.ts";
import {httpClient} from "@api/httpClient.ts";
import type {VisitWithTariff} from "@app-types/visitWithTariff.ts";

export interface CreateVisitRequest {
    userId: string;
    tariffId: string;
    plannedMinutes?: number;
    requirePositiveBalance?: boolean;
    requireEnoughForPlanned?: boolean;
}

export interface VisitResponse {
    visit: VisitWithTariff;
}

export interface VisitsResponse {
    visits: VisitWithTariff[];
}

export interface HasActiveVisitResponse {
    hasActiveVisit: boolean;
}

export interface CreateVisitResponse {
    message: string;
    visit: Visit;
}

export interface UpdateVisitResponse {
    message: string;
    visit: Visit;
}

export interface EndVisitRequest {
    visitId: string;
}

export interface EndVisitResponse {
    message: string;
    visit: Visit;
    calculatedCost: number;
}

export interface DeleteVisitResponse {
    message: string;
}

export class VisitVenueApi {
    static async getActiveVisitByUser(userId: string): Promise<VisitWithTariff> {
        try {
            const res = await httpClient.get<VisitResponse>(`/venue/visits/active/${userId}`);
            return res.data.visit;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getActiveVisit(): Promise<VisitWithTariff[]> {
        try {
            const res = await httpClient.get<VisitsResponse>(`/venue/visits/active`);
            return res.data.visits;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getVisitById(visitId: string): Promise<VisitWithTariff> {
        try {
            const res = await httpClient.get<VisitResponse>(`/venue/visits/${visitId}`);
            return res.data.visit;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async getVisitHistory(userId: string): Promise<VisitWithTariff[]> {
        try {
            const res = await httpClient.get<VisitsResponse>(`/venue/visits/history/${userId}`);
            return res.data.visits;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async hasActiveVisit(userId: string): Promise<boolean> {
        try {
            const res = await httpClient.get<HasActiveVisitResponse>(`/venue/visits/has-active/${userId}`);
            return res.data.hasActiveVisit;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }


    static async createVisit(request: CreateVisitRequest): Promise<CreateVisitResponse> {
        try {
            const res = await httpClient.post<CreateVisitResponse>(`/venue/visits`, request);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async updateVisit(visit: Visit): Promise<UpdateVisitResponse> {
        try {
            const res = await httpClient.put<UpdateVisitResponse>(`/venue/visits`, visit);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async endVisit(visitId: string): Promise<EndVisitResponse> {
        try {
            const payload: EndVisitRequest = {visitId};
            const res = await httpClient.post<EndVisitResponse>(`/venue/visits/end`, payload);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async deleteVisit(visitId: string): Promise<DeleteVisitResponse> {
        try {
            const res = await httpClient.delete<DeleteVisitResponse>(`/venue/visits/${visitId}`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

}

