import {describe, expect, it} from "vitest";
import {isNameCompleted} from "@utility/profileCompletion";
import type {Profile} from "@app-types/profile";

describe("isNameCompleted", () => {
    it("returns false when profile is null", () => {
        expect(isNameCompleted(null)).toBe(false);
    });

    it("returns false when name parts are empty", () => {
        const profile = {
            firstName: "  ",
            lastName: "Doe",
        } as Profile;

        expect(isNameCompleted(profile)).toBe(false);
    });

    it("returns true when first and last names are present", () => {
        const profile = {
            firstName: "Jane",
            lastName: "Doe",
        } as Profile;

        expect(isNameCompleted(profile)).toBe(true);
    });
});
