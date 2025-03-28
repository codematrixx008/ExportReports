import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { fetchUserDetails } from './store/authSlice';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './components/Login';
import { AppDispatch } from './store/store';
import { Profile } from './components/Profile';
import ProtectedRoute from './components/ProtectedRoute ';
import "bootstrap/dist/css/bootstrap.min.css";
import RedirectLogin from './components/RedirectLogin';
import ProtectLoginRoute from './components/ProtectLoginRoute';

const App: React.FC = () => {
    // const dispatch = useDispatch<AppDispatch>();

    // useEffect(() => {
    //     dispatch(fetchUserDetails());
    // }, [dispatch]);

    return (
        <Router>
            <Routes>
                <Route element={<ProtectLoginRoute />}>
                    <Route path="/redirectLogin" element={<RedirectLogin />} />
                    <Route path="/login" element={<Login />} />
                    <Route path="*" element={<Login />} />
                </Route>
                <Route element={<ProtectedRoute />}>
                    <Route path="/profile" element={<Profile />} />
                </Route>
            </Routes>
        </Router>
    );
};

export default App;
