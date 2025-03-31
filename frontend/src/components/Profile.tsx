import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useSelector } from "react-redux";
import { RootState } from "../store/store";
import Cookies from "js-cookie";
import Reports from "./Reports/reports";

const API_URL = "http://localhost:5280/api/Auth";

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

    // const handletest = async () => {
    //     try {
    //         await axios.post(`${API_URL}/login2`, {}, { withCredentials: true });
    //         console.log("Login successful");
    //     } catch (error) {
    //         console.error("Login failed", error);
    //     }
    // };

    const handletest = async () => {
        try {
            const response = await axios.post(`${API_URL}/login3`, {}, { withCredentials: true });

            if (response.data.redirectUrl) {
                window.location.href = response.data.redirectUrl;  // Navigate to profile page
            }
        } catch (error) {
            console.error("Login failed", error);
        }
    };

    const handleDownload = async () => {
        try {
            const response = await axios.get("http://localhost:5280/Reports/DownloadFile", {
                responseType: "blob", // Ensures proper file download
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
        <div className="container mt-5">
            <div className="row d-flex flex-wrap align-items-center">
                <div className="col-12 col-md-6 text-center text-md-start">
                    <div className="p-4">
                        <h2>Profile</h2>
                    </div>
                </div>

                <div className="col-12 col-md-6 d-flex justify-content-center justify-content-md-end">
                    <button className="btn btn-danger" onClick={handleLogout}>
                        Logout
                    </button>
                </div>
            </div>

            <div className="text-center mt-3">
                <Reports />
            </div>

            <button onClick={handleDownload}>Download Excel</button>
        </div>

    );
};

export { Profile };