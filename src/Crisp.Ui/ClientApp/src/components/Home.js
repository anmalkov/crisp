import React from 'react';
import { Link } from "react-router-dom"

const Home = () => {
    return (
        <div>
            <div className="mb-4">
                <h4>WHAT</h4>
                <p>
                    <b>CRISP</b> helps facilitate better security practices, by providing an
                    easy-to-use checklist of recommendations which can be exported into the engineering backlog as Azure
                    DevOps work items or GitHub issues.
                </p>
            </div>
            <div className="mb-4">
                <h4>HOW</h4>
                <p>
                    The process as simple as pressing a button.
                </p>
                <p>
                    Go to <Link to="/threatmodels">Security plans</Link> page and build your security plan report
                    that you want to generate in Markdown or Microsoft Word format.
                </p>
                <p>
                    Go to <Link to="/recommendations">Recommendations</Link> page and select the categories and recommendations 
                    that you want to include into your backlog.
                </p>
                <p>
                    Go to <Link to="/resources">Resources</Link> page and select the resources to generate the recommendations
                    that you want to include into your backlog.
                </p>
            </div>
        </div>
    );
};

export default Home;