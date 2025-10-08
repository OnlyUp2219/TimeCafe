import { useNavigate } from "react-router-dom";
import "./Home.css"


export const Home = () => {
    const navigate = useNavigate();
    return (
        <div className="home_root">
            <h1>Добро пожаловать! </h1>
            <p >На главную страницу </p>
        </div>
    )
}
