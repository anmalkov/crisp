import React from 'react';
import { Button, Spinner, Alert } from 'reactstrap';
import { useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from 'react-query';
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { fetchThreatModelReport } from '../fetchers/threatmodels';
import { FiArrowLeft, FiDownload } from "react-icons/fi";
import './ThreatModelReport.css';

const ThreatModelReport = () => {

    const navigate = useNavigate();

    const { state } = useLocation();
    const { id } = state;

    const { isError, isLoading, data, error } = useQuery([`threatmodelreport.${id}`], () => fetchThreatModelReport(id), { staleTime: 1 * 60 * 60 * 1000 });
    const report = data;

    if (isLoading) {
        return (
            <div className="text-center">
                <Spinner>
                    Loading...
                </Spinner>
            </div>
        );
    }

    const getReportUrl = (id) => {
        return `api/threatmodels/${id}/report/archive`;
    }

    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => navigate('/threatmodels')}><FiArrowLeft /> Back to security plans</Button>
                <a href={getReportUrl(id)} download className="btn btn-success float-end"><FiDownload /> Download report</a>
            </div>
            {isError ? (
                <Alert color="danger">{error.message}</Alert >
            ) : (
                <div id="markdown-report">
                    <ReactMarkdown remarkPlugins={[remarkGfm]}>{report}</ReactMarkdown>
                </div>
            )}
        </>
    );
}

export default ThreatModelReport;