import "axios";

declare module "axios" {
    export interface AxiosRequestConfig {
        _retry?: boolean;
        _manualRefresh?: boolean;
    }
}
