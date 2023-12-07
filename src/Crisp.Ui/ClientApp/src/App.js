import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import { QueryClient, QueryClientProvider } from 'react-query';
import { Layout } from './components/Layout';
import './custom.css';

const App = () => {

    const queryClient = new QueryClient();

    return (
        <QueryClientProvider client={queryClient}>
          <Layout>
            <Routes>
              {AppRoutes.map((route, index) => {
                const { element, ...rest } = route;
                return <Route key={index} {...rest} element={element} />;
              })}
            </Routes>
          </Layout>
        </QueryClientProvider>
    );
}

export default App;
