import React from 'react';
import { Link } from "react-router-dom"

const Home = () => {
    return (
        <div>
            <div className="mb-4">
                <p>
                    <b>Crisp</b> offers a comprehensive checklist of expert-recommended security measures, seamlessly integrating with your
                    workflow. Export these recommendations directly into your engineering backlog, whether you're using Azure DevOps or GitHub,
                    as work items or issues..
                </p>
            </div>
            <div className="mb-4">
                <h5 className="mb-4">How it works</h5>
                <p>
                    Streamlining security planning is as easy as a single click..
                </p>
                <ul>
                    <li>
                        Visit the <Link to="/threatmodels">Security plans</Link> page to effortlessly create your tailored security plan report.
                        Choose between Markdown or Microsoft Word formats for your convenience.
                    </li>
                    <li>
                        Navigate to the <Link to="/recommendations">Recommendations</Link> page to handpick categories and specific recommendations
                        for inclusion in your backlog, ensuring a customized approach to your security needs.
                    </li>
                    <li>
                        Explore the <Link to="/resources">Resources</Link> page to select essential resources for generating targeted
                        recommendations, all designed to fortify your project's security infrastructure.
                    </li>
                </ul>
            </div>
        </div>
    );
};

export default Home;