import {normalizeUnknownError} from "./normalize";
import {getUserMessage} from "./messages";

export const getUserMessageFromUnknown = (error: unknown): string => {
    return getUserMessage(normalizeUnknownError(error));
};
