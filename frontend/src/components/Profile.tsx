import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useSelector } from "react-redux";
import { RootState } from "../store/store";
import Cookies from "js-cookie";
import Reports from "./Reports/reports";

const API_URL = "http://localhost:5280/api/Auth";
const LOGO_URL = "/assets/logo.jpg";  // Logo file path
const userName = "John Cena"; // Replace with dynamic username

const Profile = () => {
    const navigate = useNavigate();
    const accessToken = useSelector((state: RootState) => state.auth.accessToken);

    const handleLogout = async () => {
        try {
            await axios.post(`${API_URL}/logout`, {}, { withCredentials: true });
            navigate("/login");
            Cookies.remove("AccessToken");
            Cookies.remove("RefreshToken");
        }
        catch (error) {
            console.error("Logout failed", error);
        }
    };

    const handleDownload = async () => {
        try {
            const response = await axios.get("http://localhost:5280/Reports/DownloadFile", {
                responseType: "blob",
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement("a");
            link.href = url;
            link.setAttribute("download", "DownloadedFile.xlsx");
            document.body.appendChild(link);
            link.click();
            link.remove();
        } catch (error) {
            alert("File path is not set or file does not exist.");
        }
    };

    return (
        <div className="container mt-3">
            {/* Navbar */}
            <div className="d-flex justify-content-between align-items-center p-3 border-bottom">
                <img src={LOGO_URL} alt="Logo" className="logo" style={{ height: "100px" }} />
                <div className="d-flex align-items-center">
                    <span className="me-3 fw-bold">{userName}</span>
                  
                </div>
            </div>

            {/* Profile Section */}
            {/* <div className="mt-4 text-center">
                <h2>Profile</h2>
                <button className="btn btn-danger" onClick={handleLogout}>
                        Logout
                    </button>
            </div> */}

            {/* Reports Section */}
            <div className="text-center mt-3">
                <Reports />
            </div>

            {/* Download Button */}
            {/* <div className="text-center mt-3">
                <button className="btn btn-primary" onClick={handleDownload}>
                    Download Excel
                </button>
            </div> */}
        </div>
    );
};

export { Profile };
