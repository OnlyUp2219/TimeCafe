import { useNavigate } from "react-router-dom";


export const Home = () => {
    const navigate = useNavigate();
    return (
        <div className="home-root">
            <h1>Добро пожаловать! </h1>
        </div>
    )
}
