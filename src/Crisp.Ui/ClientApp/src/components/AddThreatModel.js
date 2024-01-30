import React, { useState } from 'react';
import { Spinner, ListGroup, Alert, Button, Badge, FormGroup, Label, Input, Row, Col, UncontrolledAlert, CloseButton, Tooltip } from 'reactstrap';
import { useNavigate, useLocation } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { fetchThreatModelCategory, createThreatModel, fetchThreatModelFiles, updateThreatModel } from '../fetchers/threatmodels';
import { fetchResources } from '../fetchers/resources';
import { FiPlus, FiArrowLeft, FiCheck, FiInfo } from "react-icons/fi";
import Category from './Category';
import { useEffect } from 'react';
import './AddThreatModel.css';

const AddThreatModel = () => {

    const navigate = useNavigate();

    const { state } = useLocation();
    const oldThreatModel = state ? state.threatModel : null;

    const queryName = oldThreatModel ? `threatmodel-images-${oldThreatModel.id}` : 'threatmodel-images';
    const imagesQuery = useQuery([queryName], async () => await fetchThreatModelFiles(oldThreatModel ? oldThreatModel.id : null, oldThreatModel ? oldThreatModel.images : null));

    const { isError, isLoading, data, error } = useQuery(['threatmodel-category'], fetchThreatModelCategory, { staleTime: 24 * 60 * 60 * 1000 });
    const category = data;

    const resourceNames = useQuery([`fetchResources`], fetchResources, { staleTime: 1 * 60 * 60 * 1000 });

    const [selectedResources, setSelectedResources] = useState(oldThreatModel ? oldThreatModel.resources ? oldThreatModel.resources : [] : []);
    const [selectedList, setSelectedList] = useState([]);
    const [projectName, setProjectName] = useState(oldThreatModel ? oldThreatModel.projectName : '');
    const [dataflowAttributes, setDataflowAttributes] = useState(oldThreatModel ? oldThreatModel.dataflowAttributes : []);
    const [saveButtonDisabled, setSaveButtonDisabled] = useState(true);
    const [images, setImages] = useState([]);
    const [imagesDownloaded, setImagesDownloaded] = useState(false);
    const [addResourcesRecommendations, setAddResourcesRecommendations] = useState(oldThreatModel ? oldThreatModel.addResourcesRecommendations : false);

    useEffect(() => {
        if (imagesDownloaded || !oldThreatModel || !oldThreatModel.images || imagesQuery.isLoading || !imagesQuery.data || imagesQuery.data.length === 0)  {
            return;
        }
        setImages(imagesQuery.data.map(f => ({ type: f.type, fileName: f.name, file: null, url: URL.createObjectURL(f.content) })));
        setImagesDownloaded(true);
    }, [imagesQuery.data]);

    const queryClient = useQueryClient();

    const createThreatModelMutation = useMutation(threatModel => {
        return createThreatModel(threatModel, images);
    });

    const updateThreatModelMutation = useMutation(threatModel => {
        return updateThreatModel(oldThreatModel.id, threatModel, images);
    });

    const getChildrenIds = category => {
        let ids = [category.id];
        if (category.children) {
            category.children.forEach(c => ids = [...ids, ...getChildrenIds(c)]);
        }
        if (category.recommendations) {
            category.recommendations.forEach(r => ids = [...ids, r.id]);
        }
        return ids;
    }

    const toggleSelectability = selectedCategory => {
        const toggledIds = getChildrenIds(selectedCategory);
        if (selectedList.includes(selectedCategory.id)) {
            setSelectedList(selectedList.filter(id => !toggledIds.includes(id)));
        } else {
            setSelectedList([...selectedList, ...toggledIds.filter(id => !selectedList.includes(id))]);
        }
    }

    const isSelected = id => {
        return selectedList.includes(id);
    }

    const getSelectedRecommendations = (category) => {
        if (!category) {
            return [];
        }
        let recommendations = [...category.recommendations.filter(r => isSelected(r.id))];
        category.children.forEach(c => recommendations = [...recommendations, ...getSelectedRecommendations(c)]);
        return recommendations;
    }

    const selectedRecommendations = getSelectedRecommendations(category);
    const selectedRecommendationsCount = selectedRecommendations.length;

    const handleProjectNameChange = (e) => {
        setProjectName(e.target.value);
    };

    const addDataflowAttributeHandler = () => {
        let nextIndex = 1;
        if (dataflowAttributes.length > 0) {
            nextIndex = Math.max(...dataflowAttributes.map(a => a.number)) + 1;
        }
        const newAttribute = {
            number: nextIndex.toString(),
            transport: 'HTTPS/TLS 1.2',
            dataClassification: 'Confidential',
            authentication: 'Microsoft Entra ID',
            authorization: 'RBAC',
            notes: ''
        };
        setDataflowAttributes([...dataflowAttributes, newAttribute]);
    }

    const deleteDataflowAttributeHandler = (index) => {
        const attributes = [...dataflowAttributes];
        attributes.splice(index, 1);
        setDataflowAttributes(attributes);
    }

    const handleDataflowAttributeChange = (e, index) => {
        const { name, value } = e.target;
        const updatedObject = Object.assign({}, dataflowAttributes[index], { [name]: value });
        setDataflowAttributes([
            ...dataflowAttributes.slice(0, index),
            updatedObject,
            ...dataflowAttributes.slice(index + 1)
        ]);
    }

    const saveThreatModelHandler = async () => {
        const threatModel = {
            projectName: projectName,
            dataflowAttributes: dataflowAttributes,
            addResourcesRecommendations: addResourcesRecommendations,
            threats: selectedRecommendations,
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

    function onDiagramChange(type, e) {
        const newImages = images.filter(i => i.type != type);
        if (e && e.target && e.target.files[0]) {
            const file = e.target.files[0];
            newImages.push({ type: type, fileName: file.name, file: file, url: URL.createObjectURL(file) });
        } else {
            const element = document.getElementById(`image-${type}`);
            element.value = null;
        }
        setImages(newImages);
    }

    const changeResourcesHandler = (resourceName) => {
        if (selectedResources.includes(resourceName)) {
            setSelectedResources(selectedResources.filter(n => n !== resourceName));
        } else {
            setSelectedResources([...selectedResources, resourceName]);
        }
    };

    const resourceButtonColor = (resourceName) => {
        return selectedResources.includes(resourceName) ? 'primary' : 'secondary'
    }

    useEffect(() => {
        console.log(images);
    }, [images]);

    useEffect(() => {
        if (!data) {
            return;
        }
        if (selectedList.length === 0 && oldThreatModel) {
            setSelectedList(oldThreatModel.threats.map(t => t.id));
        } else {
            setSelectedList(getChildrenIds(data));
        }
    }, [data]);

    useEffect(() => {
        setSaveButtonDisabled(!projectName || projectName.length === 0 ||
            dataflowAttributes.length === 0 || selectedRecommendationsCount === 0);
    }, [projectName, dataflowAttributes, selectedList]);

    const [dataClassificationTooltipOpen, setDataClassificationTooltipOpen] = useState(false);
    const dataClassificationTooltipToggle = () => setDataClassificationTooltipOpen(!dataClassificationTooltipOpen);

    if (isLoading) {
        return (
            <div className="text-center">
                <Spinner>Loading...</Spinner>
            </div>
        );
    }

    if (isError) {
        return (
            <Alert color="danger">{error.message}</Alert >
        );
    }
    
    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => navigate('/threatmodels')}><FiArrowLeft /> Back to security plans</Button>
            </div>
            <FormGroup>
                <Label for="projectName">Project name</Label>
                <Input id="projectName" name="projectName" placeholder="Enter the project name" value={projectName} onChange={handleProjectNameChange} />
            </FormGroup>
            <FormGroup>
                <h5>Architecture diagram</h5>
                <input id="image-arch" className="form-control mb-3" type="file" accept="image/*" onChange={(e) => onDiagramChange('arch', e)} />
                {images.filter(i => i.type === 'arch').map(i => (
                    <div key={i.type} className="position-relative">
                        <img className="diagram mb-3" src={i.url} />
                        <CloseButton className="position-absolute top-0 end-0 border border-dark bg-white" onClick={() => onDiagramChange('arch')} />
                    </div>
                ))}
            </FormGroup>
            <FormGroup>
                <div className="mb-4">
                    <h5>Resources</h5>
                </div>
                <Row>
                    {resourceNames.isLoading ? (
                        <div className="text-center">
                            <Spinner>
                                Loading...
                            </Spinner>
                        </div>
                    ) : resourceNames.data.resources.map(r => (
                        <Col key={r}><Button className="resource-small" onClick={() => changeResourcesHandler(r)} color={resourceButtonColor(r)}>{r}</Button></Col>
                    ))}
                </Row>
            </FormGroup>
            <FormGroup>
                <h5>Data flow diagram</h5>
                <input id="image-flow" className="form-control mb-3" type="file" accept="image/*" onChange={(e) => onDiagramChange('flow', e)} />
                {images.filter(i => i.type === 'flow').map(i => (
                    <div key={i.type} className="position-relative">
                        <img className="diagram mb-3" src={i.url} />
                        <CloseButton className="position-absolute top-0 end-0 border border-dark bg-white" onClick={() => onDiagramChange('flow')} />
                    </div>
                ))}
            </FormGroup>
            <FormGroup>
                <h5>Data flow attributes</h5>
                <div>
                    <Button color="success" onClick={addDataflowAttributeHandler}><FiPlus /> Add attribute</Button>
                    <Row className="mt-3">
                        <Col md={1}>
                            <Label>#</Label>
                        </Col>
                        <Col className="ps-0">
                            <Label>Transport Protocol</Label>
                        </Col>
                        <Col>
                            <Label>Data Classification <FiInfo id="data-classification-info" /></Label>
                        </Col>
                        <Col>
                            <Label>Authentication</Label>
                        </Col>
                        <Col>
                            <Label>Authorization</Label>
                        </Col>
                        <Col md={5}>
                            <Label>Notes</Label>
                        </Col>
                    </Row>
                    {dataflowAttributes.map((a, index) => (
                        <Row key={index} className="mb-1">
                            <Col md={1}>
                                <Input name="number" value={a.number} onChange={(e) => handleDataflowAttributeChange(e, index)} />
                            </Col>
                            <Col className="ps-0">
                                <Input name="transport" value={a.transport} onChange={(e) => handleDataflowAttributeChange(e, index)} />
                            </Col>
                            <Col>
                                <Input type="select" name="dataClassification" value={a.dataClassification} onChange={(e) => handleDataflowAttributeChange(e, index)}>
                                    <option>Sensitive</option>
                                    <option>Confidential</option>
                                    <option>Private</option>
                                    <option>Proprietary</option>
                                    <option>Public</option>
                                </Input>
                            </Col>
                            <Col>
                                <Input name="authentication" value={a.authentication} onChange={(e) => handleDataflowAttributeChange(e, index)} />
                            </Col>
                            <Col>
                                <Input name="authorization" value={a.authorization} onChange={(e) => handleDataflowAttributeChange(e, index)} />
                            </Col>
                            <Col md={5}>
                                <Row>
                                    <Col md={11}>
                                        <Input name="notes" type="textarea" value={a.notes} onChange={(e) => handleDataflowAttributeChange(e, index)} />
                                    </Col>
                                    <Col md={1} className="ps-0">
                                        <Button color="danger" outline onClick={() => deleteDataflowAttributeHandler(index)}>X</Button>
                                    </Col>
                                </Row>
                            </Col>
                        </Row>
                    ))
                    }
                    <Tooltip isOpen={dataClassificationTooltipOpen} target="data-classification-info" toggle={dataClassificationTooltipToggle}>
                        <ul>
                            <li><b>Sensitive</b><br/>Data that is to have the most limited access and requires a high degree of integrity. This is typically data that will do the most damage to the organization should it be disclosed. Personal data (including PII) falls into this category and includes any identifier, such as name, an identification number, location data, online identifier. This also includes data related to one or more factors specific to the physical, psychological, genetic, mental, economic, cultural, or social identity of an individual.</li>
                            <li><b>Confidential</b><br/>Data that might be less restrictive within the company but might cause damage if disclosed.</li>
                            <li><b>Private</b><br/>Private data is usually compartmental data that might not do the company damage but must be kept private for other reasons. Human resources data is one example of data that can be classified as private.</li>
                            <li><b>Proprietary</b><br/>Proprietary data is data that is disclosed outside the company on a limited basis or contains information that could reduce the company's competitive advantage, such as the technical specifications of a new product.</li>
                            <li><b>Public</b><br/>Public data is the least sensitive data used by the company and would cause the least harm if disclosed. This could be anything from data used for marketing to the number of employees in the company.</li>
                        </ul>
                    </Tooltip>
                </div>
            </FormGroup>
            <FormGroup>
                <h5>Threat map</h5>
                <input id="image-map" className="form-control mb-3" type="file" accept="image/*" onChange={(e) => onDiagramChange('map', e)} />
                {images.filter(i => i.type === 'map').map(i => (
                    <div key={i.type} className="position-relative">
                        <img className="diagram mb-3" src={i.url} />
                        <CloseButton className="position-absolute top-0 end-0 border border-dark bg-white" onClick={() => onDiagramChange('map')} />
                    </div>
                ))}
            </FormGroup>
            <FormGroup>
                <h5>Threats and Mitigations</h5>
                {!category ? (
                    <p>There are no recommendations</p>
                ) : (
                    <>
                        {selectedRecommendationsCount > 0 ? (
                            <div className="d-flex justify-content-between align-items-center py-2 px-3 border-bottom border-3 border-dark mb-2 bg-super-light">
                                <span>Selected threats <Badge color="primary" className="ms-2 fs-little-smaller">{selectedRecommendationsCount}</Badge></span>
                            </div>
                        ) : null}
                        <ListGroup flush>
                            <Category category={category} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                        </ListGroup>
                    </>
                )}
            </FormGroup>
            {selectedRecommendationsCount > 0 ? (
                <FormGroup switch className="mb-3">
                    <Input className="form-check-input me-3" type="switch" role="switch" checked={addResourcesRecommendations} onChange={() => setAddResourcesRecommendations(!addResourcesRecommendations)} /> Add resources recommendations to threats
                </FormGroup>
            ): null}
            <FormGroup className="border-top border-3 border-dark pt-3">
                <Button color="success" onClick={saveThreatModelHandler} disabled={saveButtonDisabled}><FiCheck /> Save security plan</Button>
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

export default AddThreatModel;
