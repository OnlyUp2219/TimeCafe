interface MockCallbackLinkProps { url?: string }
export function MockCallbackLink({url}: MockCallbackLinkProps) {
    if (!url) return null;
    return <div style={{wordBreak:'break-all', fontSize:12, marginBottom:8}}>{url}</div>;
}
