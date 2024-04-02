import React, { useState, useEffect } from 'react';
import { Button, FormGroup, Label, Input } from 'reactstrap';
import { FiArrowLeft, FiCheck } from "react-icons/fi";
import MarkdownEditor from '../threats/MarkdownEditor';

const ThreatRecommendation = ({ recommendation, onClose }) => {

    const [recommendationLocal, setRecommendationLocal] = useState(recommendation);
    const [saveButtonDisabled, setSaveButtonDisabled] = useState(false);

    const onModelPropertyChange = (e) => {
        const { name, value } = e.target;
        setRecommendationLocal(prev => ({ ...prev, [name]: value }));
    }

    useEffect(() => {
        const isDisabled = !recommendationLocal.title.trim() || !recommendationLocal.description.trim();
        setSaveButtonDisabled(isDisabled);
    }, [recommendationLocal.title, recommendationLocal.description]);

    return (
        <>
            <div className="mb-3">
                <Button color="secondary" onClick={() => onClose(null)}><FiArrowLeft /> Back to threat</Button>
            </div>
            <h5>{recommendation && recommendation.id ? "Update recommendation" : "New recommendation"}</h5>
            <FormGroup>
                <Label for="title">Title</Label>
                <Input id="title" name="title" placeholder="Enter the recommendation title" value={recommendationLocal.title} onChange={onModelPropertyChange} />
            </FormGroup>
            <MarkdownEditor
                label="Description"
                placeholder="Enter the recommendation description"
                value={recommendationLocal.description}
                setValue={(value) => onModelPropertyChange({ target: { name: "description", value: value } })}
            />
            <FormGroup className="border-top border-3 border-dark pt-3">
                <Button color="success" onClick={() => onClose(recommendationLocal)} disabled={saveButtonDisabled}><FiCheck /> Save recommendation</Button>
            </FormGroup>
        </>
    );
};

export default ThreatRecommendation;
