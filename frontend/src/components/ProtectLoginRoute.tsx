import { Navigate, Outlet } from "react-router-dom";
import Cookies from "js-cookie";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../store/store";
import { useState, useEffect } from "react";
import { fetchUserDetails } from "../store/authSlice";

const ProtectLoginRoute = () => {
    const dispatch = useDispatch();
    const user = useSelector((state: RootState) => state.auth.user);
    const loading = useSelector((state: RootState) => state.auth.loading);
    console.log("ProtectLoginRoute user:", user);
    
    const [isUserFetched, setIsUserFetched] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            if (!user && !isUserFetched) {
                await dispatch(fetchUserDetails() as any);   // Fetch user on protected route access
                setIsUserFetched(true);                     // Prevent redundant API calls
            }
        };

        fetchData();
    }, [dispatch, user, isUserFetched]);

    // Show loading indicator while fetching the user
    if (loading && !user) {
        return <div>Loading...</div>;
    }    

    if(user){
        return <Navigate to="/profile" />;
    }
    
    return  <Outlet /> ;
};

export default ProtectLoginRoute;