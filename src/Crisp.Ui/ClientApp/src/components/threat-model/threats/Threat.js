import React, { useState, useEffect } from 'react';
import { Button, FormGroup, Label, Input } from 'reactstrap';
import { FiArrowLeft, FiCheck } from "react-icons/fi";
import MarkdownEditor from './MarkdownEditor';
import ThreatRecommendations from '../threat-recommendations/ThreatRecommendations';

const Threat = ({ threat, onClose, resources }) => {

    const [threatLocal, setThreatLocal] = useState(threat);
    const [recommendations, setRecommendations] = useState(threat?.recommendations || []);
    const [saveButtonDisabled, setSaveButtonDisabled] = useState(false);

    const onModelPropertyChange = (e) => {
        const { name, value } = e.target;

        const isNumericField = name === 'status' || name === 'risk';
        const typedValue = isNumericField ? parseInt(value, 10) : value;

        setThreatLocal(prev => ({ ...prev, [name]: typedValue }));
    }

    useEffect(() => {
        const isDisabled = !threatLocal.title.trim() || !threatLocal.description.trim();
        setSaveButtonDisabled(isDisabled);
    }, [threatLocal.title, threatLocal.description]);

    useEffect(() => {
        setThreatLocal(prev => ({ ...prev, recommendations }));
    }, [recommendations]);

    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => onClose(null)}><FiArrowLeft /> Back to security plan</Button>
            </div>
            <h5>{threat && threat.id ? "Update threat" : "New threat"}</h5>
            <FormGroup>
                <Label for="title">Title</Label>
                <Input id="title" name="title" placeholder="Enter the threat title" value={threatLocal.title} onChange={onModelPropertyChange} />
            </FormGroup>
            <MarkdownEditor
                label="Description"
                placeholder="Enter the threat description"
                value={threatLocal.description}
                setValue={(value) => onModelPropertyChange({ target: { name: "description", value: value } })}
            />
            <FormGroup>
                <Label for="status">Status</Label>
                <Input type="select" id="status" name="status" value={threatLocal.status} onChange={onModelPropertyChange}>
                    <option value="0">Not evaluated</option>
                    <option value="1">Not mitigated</option>
                    <option value="2">Partially mitigated</option>
                    <option value="3">Mitigated</option>
                </Input>
            </FormGroup>
            <FormGroup>
                <Label for="risk">Risk</Label>
                <Input type="select" id="risk" name="risk" value={threatLocal.risk} onChange={onModelPropertyChange}>
                    <option value="0">Not evaluated</option>
                    <option value="1">Critical</option>
                    <option value="2">High</option>
                    <option value="3">Medium</option>
                    <option value="4">Low</option>
                </Input>
            </FormGroup>
            <FormGroup>
                <h5>Recommendations</h5>
                <ThreatRecommendations recommendations={recommendations} setRecommendations={setRecommendations} resources={resources} />
            </FormGroup>
            <FormGroup className="border-top border-3 border-dark pt-3">
                <Button color="success" onClick={() => onClose(threatLocal)} disabled={saveButtonDisabled}><FiCheck /> Save threat</Button>
            </FormGroup>
        </>
    );
};

export default Threat;
