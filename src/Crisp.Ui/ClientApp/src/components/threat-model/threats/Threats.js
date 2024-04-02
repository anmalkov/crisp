import React, { useState, useMemo, useEffect } from 'react';
import { Button, Table, Dropdown, DropdownToggle, DropdownMenu, DropdownItem, Modal, ModalBody } from 'reactstrap';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { FiPlus, FiX } from "react-icons/fi";
import { RxDragHandleDots2 } from "react-icons/rx";
import { fetchRecommendations } from '../../../fetchers/resources';
import ThreatStatus from './ThreatStatus';
import ThreatRisk from './ThreatRisk';
import SelectThreats from './SelectThreats';
import Threat from './Threat';

const Threats = ({ threats, setThreats, addResourcesRecommendations, resources }) => {

    const [addThreatMenuOpen, setAddThreatMenuOpen] = useState(false);
    const [selectThreatsModalOpen, setSelectThreatsModalOpen] = useState(false);
    const [threatModalOpen, setThreatModalOpen] = useState(false);
    const [selectedThreat, setSelectedThreat] = useState(null);
    //const [resourcesRecommendations, setResourcesRecommendations] = useState([]);

    const sortedThreats = useMemo(() => {
        return [...threats].sort((a, b) => a.orderIndex - b.orderIndex);
    }, [threats]);

    //useEffect(() => {
    //    const fetchAndSetRecommendations = async () => {
    //        const recommendations = await fetchRecommendations(resources);
    //        console.log(recommendations);
    //        setResourcesRecommendations(recommendations);
    //    }
    //    fetchAndSetRecommendations();
    //}, [resources]);

    const defaultDescription = `**Principle:** Confidentiality, Integrity, and Availability
**Affected Asset:** All services  
**Threat:** [threat description].

**Mitigation:**

[mitigation description]

1. Mitigation step 1.
2. Mitigation step 2.
3. Mitigation step 3.
`;

    const defaultThreat = {
        title: '',
        description: defaultDescription,
        status: 0,  // Not evaluated
        risk: 0,  // Not evaluated
        orderIndex: getNextOrderIndex(),
        recommendations: []
    };

    function getNextOrderIndex() {
        return threats.length > 0 ? Math.max(...threats.map(t => t.orderIndex)) + 1 : 1;
    }

    const addOrUpdateThreat = (threat) => {
        setSelectedThreat(threat);
        setThreatModalOpen(true);
    }

    const deleteThreat = (e, threat) => {
        e.stopPropagation();
        if (!window.confirm(`Do you want to delete threat '${threat.title}' ?`)) {
            return;
        }
        setThreats(prev => prev.filter(t => t.id !== threat.id)
            .map(t => t.orderIndex > threat.orderIndex ? { ...t, orderIndex: t.orderIndex - 1 } : t));
    }

    const addThreatsFromCatalog = () => {
        setSelectThreatsModalOpen(true);
    }

    const toggleAddThreatMenu = () => setAddThreatMenuOpen((prevState) => !prevState);

    const getRecommendationsForResources = (threat) => {
        const recommendations = [];
        if (!addResourcesRecommendations || resources.length === 0) {
            return recommendations;
        }
        //console.log(threat);
        return recommendations;
    }

    const getRecommendations = (threat) => {
        const recommendations = getRecommendationsForResources(threat);
        return recommendations;
    }

    const toggleSelectThreatsModal = (selectedThreats) => {
        if (selectedThreats) {
            let orderIndex = getNextOrderIndex();
            const threatsToAdd = [];
            selectedThreats.filter(t => !threats.find(threat => threat.id === t.id)).forEach(t => {
                threatsToAdd.push({
                    id: t.id,
                    title: t.title,
                    description: t.description,
                    status: 0,  // Not evaluated
                    risk: 0,  // Not evaluated
                    orderIndex: orderIndex,
                    recommendations: getRecommendations(t)
                });
                orderIndex++;
            });
            setThreats(prev => [...prev, ...threatsToAdd]);
        }
        setSelectThreatsModalOpen(prev => !prev);
    }

    const toggleThreatModal = (threat) => {
        if (threat) {
            if (threat.id) {
                setThreats(prev => {
                    const index = prev.findIndex(t => t.id === threat.id);
                    prev[index] = threat;
                    return [...prev];
                });

                setSelectedThreat(null);
            } else {
                threat.id = Math.floor(Date.now() / 1000).toString();
                console.log(threat.id);
                setThreats(prev => [...prev, threat]);
            }
        }
        setThreatModalOpen(prev => !prev);
    }

    const onDragEnd = (result) => {
        if (!result.destination || result.destination.index === result.source.index) {
            return;
        }

        const threatMovedForward = result.destination.index > result.source.index;
        const newOrderIndex = result.destination.index + 1;

        setThreats(prev => prev.map(t => {
            if (t.id === result.draggableId) {
                return { ...t, orderIndex: newOrderIndex };
            } else if (threatMovedForward) {
                if (t.orderIndex > result.source.index + 1 && t.orderIndex <= newOrderIndex) {
                    return { ...t, orderIndex: t.orderIndex - 1 };
                }
            } else {
                if (t.orderIndex >= newOrderIndex && t.orderIndex < result.source.index + 1) {
                    return { ...t, orderIndex: t.orderIndex + 1 };
                }
            }
            return t;
        }));
    };
    
    return (
        <>
            <div>
                <Dropdown isOpen={addThreatMenuOpen} toggle={toggleAddThreatMenu}>
                    <DropdownToggle caret color="success"><FiPlus /> Add threat  &nbsp; </DropdownToggle>
                    <DropdownMenu>
                        <DropdownItem onClick={() => addOrUpdateThreat(defaultThreat)}>New threat</DropdownItem>
                        <DropdownItem onClick={addThreatsFromCatalog}>From catalog</DropdownItem>
                    </DropdownMenu>
                </Dropdown>
            </div>
            {sortedThreats.length > 0 ? (
                <DragDropContext onDragEnd={onDragEnd}>
                    <Droppable droppableId="droppable" direction="vertical">
                        {(provided) => (
                            <Table hover>
                                <thead>
                                    <tr>
                                        <th style={{ width: '70px', minWidth: '70px', maxWidth: '70px' }}></th>
                                        <th scope="col" className="w-50">Title</th>
                                        <th scope="col">Status</th>
                                        <th scope="col">Risk</th>
                                        <th style={{ width: '70px', minWidth: '70px', maxWidth: '70px' }}></th>
                                    </tr>
                                </thead>
                                <tbody ref={provided.innerRef} {...provided.droppableProps}>
                                    {sortedThreats.map((threat, index) => (
                                        <Draggable key={threat.id} draggableId={threat.id} index={index}>
                                            {(provided, snapshot) => (
                                                <tr
                                                    ref={provided.innerRef}
                                                    {...provided.draggableProps}
                                                    onClick={() => addOrUpdateThreat(threat)}
                                                    className={`cursor-pointer align-middle ${snapshot.isDragging ? "dragging" : ""}`}
                                                >
                                                    <td {...provided.dragHandleProps} className="drag-handle"><RxDragHandleDots2 /> {threat.orderIndex}</td>
                                                    <td><Button color="link" className="text-start">{threat.title}</Button></td>
                                                    <td><ThreatStatus status={threat.status} /></td>
                                                    <td><ThreatRisk risk={threat.risk} /></td>
                                                    <td>
                                                        <div className="hstack gap-3 float-end">
                                                            <Button size="sm" outline color="danger" onClick={(e) => deleteThreat(e, threat)}><FiX /></Button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            )}
                                        </Draggable>
                                    ))}
                                    {provided.placeholder}
                                </tbody>
                            </Table>
                        )}
                    </Droppable>
                </DragDropContext>
            ) : null }
            <Modal isOpen={selectThreatsModalOpen} toggle={toggleSelectThreatsModal} fullscreen>
                <ModalBody>
                    <SelectThreats threats={threats} setThreats={setThreats} onClose={toggleSelectThreatsModal} />
                </ModalBody>
            </Modal>
            <Modal isOpen={threatModalOpen} toggle={toggleThreatModal} fullscreen>
                <ModalBody>
                    <Threat threat={selectedThreat} onClose={toggleThreatModal} resources={resources} />
                </ModalBody>
            </Modal>
        </>
    );
};

export default Threats;
