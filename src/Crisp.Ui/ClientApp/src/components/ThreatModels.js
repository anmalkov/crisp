import React, { useMemo } from 'react';
import { Spinner, Alert, Button, Table } from 'reactstrap';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { fetchThreatModels, deleteThreatModel } from '../fetchers/threatmodels';
import { useNavigate } from 'react-router-dom';
import { FiEdit2, FiDownload, FiX, FiSearch, FiPlus } from "react-icons/fi";
import ThreatModelStatusBar from './ThreatModelStatusBar';

const ThreatModels = () => {

    const navigate = useNavigate();

    const threatModels = useQuery(['threatmodels'], fetchThreatModels, { staleTime: 1 * 60 * 60 * 1000 });

    const sortedThreatModels = useMemo(() => {
        return threatModels && threatModels.data ? [...threatModels.data].sort((a, b) => a.projectName > b.projectName ? 1 : -1) : [];
    }, [threatModels]);

    const queryClient = useQueryClient();

    const deleteThreatModelMutation = useMutation(id => {
        return deleteThreatModel(id);
    });

    const getReportUrl = (id) => {
        return `api/threatmodels/${id}/report/archive`;
    }

    const addUpdateThreatModel = (threatModel) => {
        navigate('/addthreatmodel', threatModel ? { state: { threatModel: threatModel } } : {});
    }

    const deleteThreatModelHandler = async (e, id) => {
        e.stopPropagation();
        const threatModel = threatModels.data.find(t => t.id === id);
        if (!threatModel || !window.confirm(`Do you want to delete security plan '${threatModel.projectName}' ?`)) {
            return;
        }
        try {
            await deleteThreatModelMutation.mutateAsync(id);
            queryClient.invalidateQueries(['threatmodels']);
            queryClient.refetchQueries('threatmodels', { force: true });
        }
        catch { }
    }

    const downloadReport = (e, id) => {
        e.stopPropagation();
        window.open(getReportUrl(id), '_blank');
    }

    const navigateToReport = (e, id) => {
        e.stopPropagation();
        navigate('/threatmodelreport', { state: { id: id } });
    }

    if (threatModels?.isLoading) {
        return (
            <div className="text-center">
                <Spinner>
                    Loading...
                </Spinner>
            </div>
        );
    }

    if (threatModels?.isError) {
        return (
            <Alert color="danger">{threatModels.error.message}</Alert >
        );
    }

    return (
        <>
            <div className="mb-3">
                <Button color="success" onClick={() => addUpdateThreatModel(null)}><FiPlus /> New security plan</Button>
            </div>
            {!sortedThreatModels.length === 0 ? (
                <p>There are no security plans</p>
            ) : (
                <Table hover>
                    <thead>
                        <tr>
                            <th scope="col" className="w-50">Project name</th>
                            <th scope="col">Status</th>
                            <th scope="col">Created</th>
                            <th scope="col">Updated</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        {sortedThreatModels.map(t => (
                            <tr key={t.id} onClick={() => addUpdateThreatModel(t)} className="cursor-pointer align-middle">
                                <td><Button color="link" className="text-start">{t.projectName}</Button></td>
                                <td><ThreatModelStatusBar id={t.id} threats={t.threats} /></td>
                                <td>{(new Date(t.createdAt)).toLocaleDateString()}</td>
                                <td>{t.updatedAt ? (new Date(t.updatedAt)).toLocaleDateString() : 'Never'}</td>
                                <td>
                                    <div className="hstack gap-3 float-end">
                                        <Button size="sm" outline color="success" onClick={(e) => navigateToReport(e, t.id)}><FiSearch /></Button>
                                        <Button size="sm" outline color="success" onClick={(e) => downloadReport(e, t.id)}><FiDownload /></Button>
                                        <Button size="sm" outline color="danger" onClick={(e) => deleteThreatModelHandler(e, t.id)}><FiX /></Button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            )}
        </>
    );
};

export default ThreatModels;
