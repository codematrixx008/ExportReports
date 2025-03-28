import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';
import Cookies from 'js-cookie';

// Axios instance with cookie support
const api = axios.create({
    baseURL: 'http://localhost:5280/api',  // Replace with your .NET Core API base URL
    withCredentials: true  // Enable cookies in requests
});

// Interfaces for state and user data
interface User {
    id: string;
    username: string;
    email: string;
}

interface AuthState {
    user: User | null;
    accessToken: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    error: string | null;
}

// Initial state
const initialState: AuthState = {
    user: null,
    accessToken: Cookies.get("AccessToken") || null,
    isAuthenticated: !!Cookies.get("AccessToken"),
    loading: false,
    error: null
};

// Thunk to handle redirect login
export const redirectloginUser = createAsyncThunk(
    'auth/redirectloginUser',
    async ({ redirectToken }: { redirectToken: string; }, { rejectWithValue }) => {
        try {
            const response = await api.get('/auth/redirectloginUser', {
                headers: { Authorization: `Bearer ${redirectToken}` }
            });

            // Store token in cookies for persistent authentication
            const { accessToken, user } = response.data;

            console.log("AccessToken",accessToken);
            console.log("User",user);


            return { accessToken, user };
        }
        catch (error: any) {
            return rejectWithValue(error.response?.data?.message || "Redirect login failed");
        }
    }
);

// Thunk to handle login
export const loginUser = createAsyncThunk(
    'auth/loginUser',
    async ({ username, password }: { username: string; password: string }, { rejectWithValue }) => {
        try {
            const response = await api.post('/auth/login', { username, password });
            return response.data.accessToken;
        }
        catch (error: any) {
            return rejectWithValue(error.response?.data?.message || "Login failed");
        }
    }
);

// Thunk to fetch user details
export const fetchUserDetails = createAsyncThunk(
    'auth/fetchUserDetails',
    async (_, { rejectWithValue, getState }) => {
        try {
            const response = await api.get('/auth/MyDetails');
            return response.data.user as User;
        } catch (error: any) {
            return rejectWithValue(error.response?.data?.message || "Failed to fetch user details");
        }
    }
);

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setAccessToken: (state, action: PayloadAction<string>) => {
            state.accessToken = action.payload;
            state.isAuthenticated = true;
            Cookies.set("AccessToken", action.payload);
        },
        logout: (state) => {
            state.user = null;
            state.accessToken = null;
            state.isAuthenticated = false;
            Cookies.remove("AccessToken");
        }
    },
    extraReducers: (builder) => {
        builder
            // Handle loginUser thunk
            .addCase(loginUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(loginUser.fulfilled, (state, action) => {
                state.accessToken = action.payload;
                state.isAuthenticated = true;
                state.loading = false;
            })
            .addCase(loginUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload as string;
            })

            // Handle fetchUserDetails thunk
            .addCase(fetchUserDetails.fulfilled, (state, action: PayloadAction<User>) => {
                state.user = action.payload;
            })
            .addCase(fetchUserDetails.rejected, (state, action) => {
                state.error = action.payload as string;
            })

            // Handle redirectloginUser thunk
            .addCase(redirectloginUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(redirectloginUser.fulfilled, (state, action) => {
                state.accessToken = action.payload.accessToken;
                state.user = action.payload.user;
                state.isAuthenticated = true;
                state.loading = false;
            })
            .addCase(redirectloginUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload as string;
            });
    }
});

export const { logout, setAccessToken } = authSlice.actions;
export default authSlice.reducer;
