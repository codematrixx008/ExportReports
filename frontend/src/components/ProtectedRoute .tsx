import { Navigate, Outlet } from "react-router-dom";
import Cookies from "js-cookie";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../store/store";
import { useEffect, useState } from "react";
import { fetchUserDetails } from "../store/authSlice";

const ProtectedRoute = () => {
    const dispatch = useDispatch();
    const user = useSelector((state: RootState) => state.auth.user);
    const loading = useSelector((state: RootState) => state.auth.loading);
    console.log("ProtectedRoute user:", user);
    
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

    if(!user){
        console.log("ProtectedRoute redirecting to  Login:", user);
        return <Navigate to="/Login" replace />;
    }
    
    return  <Outlet /> ;
};

export default ProtectedRoute;