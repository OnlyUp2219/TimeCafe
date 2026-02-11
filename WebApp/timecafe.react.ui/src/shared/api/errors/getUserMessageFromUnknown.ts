import {normalizeUnknownError} from "@api/errors/normalize";
import {getUserMessage} from "@api/errors/messages";

export const getUserMessageFromUnknown = (error: unknown): string => {
    return getUserMessage(normalizeUnknownError(error));
};
