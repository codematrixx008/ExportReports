import React, { useState, useEffect } from "react";
import axios from "axios";
import { useLocation, useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { setAccessToken,redirectloginUser, fetchUserDetails } from "../store/authSlice";
import { AppDispatch } from "../store/store";

const API_BASE_URL = "http://localhost:5280/api/Auth";

const RedirectLogin: React.FC = () => {
    const [redirectToken, setRedirectToken] = useState<string>('8ce4CTEfdcTX5l0BJ5gVXcCpx0lJb0HoaeJ7tAz1UBY%3D');
    const [message, setMessage] = useState("");
    const navigate = useNavigate();
    const location = useLocation();
    const dispatch = useDispatch<AppDispatch>();

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const redirectToken = params.get("rdtkn");   // Get the "rdtkn" param

        if (redirectToken) {
            console.log("Redirect token:", redirectToken);

            // Dispatch the thunk with the token
            const login = async () => {
                try {
                    const resultAction = await dispatch(redirectloginUser({ redirectToken }) as any);

                    if (redirectloginUser.fulfilled.match(resultAction)) {
                        console.log("Login successful:", resultAction.payload);
                        await dispatch(fetchUserDetails());
                        console.log("fetching user fetchUserDetails:");
                        navigate("/profile");   // Navigate on success
                    } else {
                        console.error("Login failed:", resultAction.payload);
                        navigate("/Login");     // Redirect to login on failure
                    }
                } catch (error) {
                    console.error("Unexpected error:", error);
                    navigate("/Login");
                }
            };

            login();
        } else {
            // Redirect to login if the query param is missing
            console.warn("No token found in URL, redirecting to Login...");
            navigate("/Login");
        }
    }, [location, dispatch, navigate]);



    useEffect(() => {

    }, []);

    return (
        <div className="container d-flex justify-content-center align-items-center vh-100">
            <div className="card p-4 shadow" style={{ width: "350px" }}>
                <h2 className="text-center mb-4">Redirecting...</h2>
                <p>Please wait while we log you in.</p>
            </div>
        </div>
    );
};

export default RedirectLogin;