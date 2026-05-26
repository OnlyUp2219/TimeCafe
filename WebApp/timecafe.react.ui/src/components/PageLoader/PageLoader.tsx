import { Spinner } from "@fluentui/react-components";

interface PageLoaderProps {
    label?: string;
}

export const PageLoader = ({ label }: PageLoaderProps) => {
    return (
        <div className="flex flex-col items-center justify-center h-full">
            <Spinner size="huge" label={label} />
        </div>
    );
};
