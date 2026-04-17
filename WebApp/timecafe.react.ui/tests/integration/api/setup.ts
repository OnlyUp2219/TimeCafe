/// <reference types="node" />
import {existsSync, mkdirSync, writeFileSync} from "node:fs";
import {resolve} from "node:path";

const artifactsDir = resolve(process.cwd(), "tests", ".artifacts");
if (!existsSync(artifactsDir)) {
    mkdirSync(artifactsDir, {recursive: true});
}
const usersFile = resolve(artifactsDir, "test-users.txt");
writeFileSync(usersFile, "", "utf8");
