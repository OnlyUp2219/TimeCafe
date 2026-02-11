import {beforeAll} from "vitest";
import axios from "axios";
import {existsSync, mkdirSync, writeFileSync} from "node:fs";
import {resolve} from "node:path";
import {apiBaseUrl} from "@tests/integration/api/helpers";

beforeAll(async () => {
    const artifactsDir = resolve(process.cwd(), "tests", ".artifacts");
    if (!existsSync(artifactsDir)) {
        mkdirSync(artifactsDir, {recursive: true});
    }
    const usersFile = resolve(artifactsDir, "test-users.txt");
    writeFileSync(usersFile, "", "utf8");

    const healthUrl = new URL("/health", apiBaseUrl).toString();
    const res = await axios.get(healthUrl, {validateStatus: () => true});
    if (res.status < 200 || res.status >= 300) {
        throw new Error(`API недоступен: ${healthUrl} (${res.status})`);
    }
}, 30000);
