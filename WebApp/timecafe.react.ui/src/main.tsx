import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from "./App"
import { FluentProvider, webLightTheme } from '@fluentui/react-components'

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <FluentProvider theme={webLightTheme} style={{ width: '100%', height: '100%' }}>
            <App />
        </FluentProvider>
    </StrictMode>
)
