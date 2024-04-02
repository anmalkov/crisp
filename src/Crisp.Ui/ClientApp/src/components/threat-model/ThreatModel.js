import React, { useState, useEffect } from 'react';
import { Spinner, Button, FormGroup, Label, Input,UncontrolledAlert} from 'reactstrap';
import { useNavigate, useLocation } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { createThreatModel, fetchThreatModelFiles, updateThreatModel } from '../../fetchers/threatmodels';
import { FiArrowLeft, FiCheck } from "react-icons/fi";
import DataFlowAttributes from './DataFlowAttributes';
import ThreatModelImage from './ThreatModelImage';
import ThreatModelResources from './ThreatModelResources';
import Threats from './threats/Threats';
import './ThreatModel.css';

const ThreatModel = () => {

    const navigate = useNavigate();
    const { state } = useLocation();
    const oldThreatModel = state ? state.threatModel : null;

    const [projectName, setProjectName] = useState(oldThreatModel?.projectName ?? '');
    const [selectedResources, setSelectedResources] = useState(oldThreatModel?.resources ?? []);
    const [dataflowAttributes, setDataflowAttributes] = useState(oldThreatModel?.dataflowAttributes ?? []);
    const [addResourcesRecommendations, setAddResourcesRecommendations] = useState(oldThreatModel?.addResourcesRecommendations ?? false);
    const [images, setImages] = useState([]);
    const [threats, setThreats] = useState(oldThreatModel?.threats ?? []);
    const [imagesDownloaded, setImagesDownloaded] = useState(false);
    const [saveButtonDisabled, setSaveButtonDisabled] = useState(true);

    const queryClient = useQueryClient();
    const queryName = oldThreatModel ? `threatmodel-images-${oldThreatModel.id}` : 'threatmodel-images';
    useQuery([queryName], () => fetchThreatModelFiles(oldThreatModel?.id, oldThreatModel?.images), {
        onSuccess: (data) => {
            if (data.length > 0 && !imagesDownloaded) {
                setImages(data.map(f => ({
                    type: f.type,
                    fileName: f.name,
                    file: null,
                    url: URL.createObjectURL(f.content)
                })));
                setImagesDownloaded(true);
            }
        }
    });

    const onProjectNameChange = (e) => {
        setProjectName(e.target.value);
    };

    useEffect(() => {
        setSaveButtonDisabled(!projectName || projectName.length === 0);
    }, [projectName]);

    const createThreatModelMutation = useMutation(threatModel => {
        return createThreatModel(threatModel, images);
    });

    const updateThreatModelMutation = useMutation(threatModel => {
        return updateThreatModel(oldThreatModel.id, threatModel, images);
    });

    const saveThreatModel = async () => {
        const threatModel = {
            projectName: projectName,
            dataflowAttributes: dataflowAttributes,
            addResourcesRecommendations: addResourcesRecommendations,
            threats: threats,
            images: images.length > 0 ? images.map(i => ({ key: i.type, value: i.fileName })) : null,
            resources: selectedResources
        };
        try {
            if (!oldThreatModel) {
                await createThreatModelMutation.mutateAsync(threatModel);
            } else {
                await updateThreatModelMutation.mutateAsync(threatModel);
                queryClient.invalidateQueries([`threatmodelreport.${oldThreatModel.id}`]);
                queryClient.refetchQueries(`threatmodelreport.${oldThreatModel.id}`, { force: true });
            }
            queryClient.invalidateQueries(['threatmodels']);
            queryClient.refetchQueries('threatmodels', { force: true });
            navigate('/threatmodels');
        }
        catch { }
    }

    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => navigate('/threatmodels')}><FiArrowLeft /> Back to security plans</Button>
            </div>
            <FormGroup>
                <Label for="projectName">Project name</Label>
                <Input id="projectName" name="projectName" placeholder="Enter the project name" value={projectName} onChange={onProjectNameChange} />
            </FormGroup>
            <FormGroup>
                <h5>Architecture diagram</h5>
                <ThreatModelImage images={images} setImages={setImages} type="arch" />
            </FormGroup>
            <FormGroup>
                <h5>Resources</h5>
                <ThreatModelResources
                    selectedResources={selectedResources}
                    setSelectedResources={setSelectedResources}
                    addResourcesRecommendations={addResourcesRecommendations}
                    setAddResourcesRecommendations={setAddResourcesRecommendations} />
            </FormGroup>
            <FormGroup>
                <h5>Data flow diagram</h5>
                <ThreatModelImage images={images} setImages={setImages} type="flow" />
            </FormGroup>
            <FormGroup>
                <h5>Data flow attributes</h5>
                <DataFlowAttributes dataflowAttributes={dataflowAttributes} setDataflowAttributes={setDataflowAttributes} />
            </FormGroup>
            <FormGroup>
                <h5>Threat map</h5>
                <ThreatModelImage images={images} setImages={setImages} type="map" />
            </FormGroup>
            <FormGroup>
                <h5>Threats and Mitigations</h5>
                <Threats threats={threats} setThreats={setThreats} addResourcesRecommendations={addResourcesRecommendations} resources={selectedResources} />
            </FormGroup>
            <FormGroup className="border-top border-3 border-dark pt-3">
                <Button color="success" onClick={saveThreatModel} disabled={saveButtonDisabled}><FiCheck /> Save security plan</Button>
                {createThreatModelMutation.isLoading &&
                    <Spinner size="sm">Loading...</Spinner>
                }
                {createThreatModelMutation.isError &&
                    <UncontrolledAlert color="danger" className="m-3 mb-0">
                        {createThreatModelMutation.error.message}
                    </UncontrolledAlert>
                }
            </FormGroup>
        </>
    );
};

export default ThreatModel;
